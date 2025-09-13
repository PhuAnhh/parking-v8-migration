using Application.DbContexts.v3;
using Application.DbContexts.v8;
using Application.Entities;
using Application.Entities.v3;
using Application.Entities.v8;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

namespace Application.Services;

public class InsertEntriesService
{
    private readonly MParkingEventDbContext _mParkingEventDbContext;
    private readonly MParkingDbContext _mParkingDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;
    private readonly MinioSettings _minioSettings;
    private readonly IMinioClient _minioClient;

    public InsertEntriesService(
        MParkingEventDbContext mParkingEventDbContext,
        MParkingDbContext mParkingDbContext,
        EventDbContext eventDbContext,
        ResourceDbContext resourceDbContext,
        MinioSettings minioSettings)
    {
        _mParkingEventDbContext = mParkingEventDbContext;
        _mParkingDbContext = mParkingDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
        _minioSettings = minioSettings;

        _minioClient = new MinioClient()
            .WithEndpoint(_minioSettings.Endpoint)
            .WithCredentials(_minioSettings.AccessKey, _minioSettings.SecretKey)
            .Build();
    }

    public async Task InsertEntries(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        log($"Successfully connected to MinIO bucket [{_minioSettings.Bucket}]!");

        int insertedEntries = 0;
        int skippedEntries = 0;

        await EnsureBucketExists(log);

        var cards = await _mParkingDbContext.Cards
            .Where(c => !c.IsLock && !c.IsDelete)
            .ToListAsync();

        var cardGroups = await _mParkingDbContext.CardGroups
            .Select(cg => new
            {
                CardGroupID = cg.CardGroupID.ToString(),
            })
            .ToListAsync();

        var entryCardEvents = await _mParkingEventDbContext.EntryCardEventDtos
            .Where(e => e.EventCode == "1" && e.DatetimeIn >= fromDate)
            .ToListAsync();

        var cardEvents = (from c in cards
            join e in entryCardEvents on c.CardNumber equals e.CardNumber
            join cg in cardGroups on c.CardGroupID equals cg.CardGroupID into cgGroup
            from cg in cgGroup.DefaultIfEmpty()
            select new CardEvent
            {
                Id = e.Id,
                CardNumber = e.CardNumber,
                PlateIn = e.PlateIn,
                EventCode = e.EventCode,
                DatetimeIn = e.DatetimeIn,
                PicDirIn = e.PicDirIn,
                LaneIDIn = e.LaneIDIn,
                UserIDIn = e.UserIDIn,
                CustomerName = e.CustomerName,
                Moneys = e.Moneys,
                IsDelete = e.IsDelete
            }).ToList();

        foreach (var ce in cardEvents)
        {
            token.ThrowIfCancellationRequested();

            var accessKey = await MigrateAccessKey(ce, log);
            if (accessKey == null)
                continue;

            var device = await MigrateDevice(ce, log);

            var customer = await MigrateCustomer(ce, log);

            var existedEntry = await _eventDbContext.Entries.AnyAsync(e => e.Id == ce.Id);
            if (!existedEntry)
            {
                var entry = new Entry
                {
                    Id = ce.Id,
                    PlateNumber = ce.PlateIn,
                    DeviceId = device.Id,
                    AccessKeyId = accessKey.Id,
                    Exited = false,
                    Amount = (long)ce.Moneys,
                    Deleted = false,
                    CreatedBy = "admin",
                    CreatedUtc = TimeZoneInfo.ConvertTimeToUtc(ce.DatetimeIn,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    CustomerId = customer?.Id,
                };
                
                _eventDbContext.Entries.Add(entry);
                await _eventDbContext.SaveChangesAsync();

                insertedEntries++;

                if (!string.IsNullOrEmpty(ce.PicDirIn))
                {
                    await ProcessEntryImages(ce, entry.CreatedUtc, log);
                }
            }
            else
            {
                skippedEntries++;
            }
        }

        log("---------- Processing Result ----------");
        log($"Total number of events: {cardEvents.Count}");
        log($"Events successfully migrated: {insertedEntries}");
        log($"Events already existed (skipped): {skippedEntries}");
    }

    private async Task ProcessEntryImages(CardEvent cardEvent, DateTime createdUtc, Action<string> log)
    {
        try
        {
            await ProcessImageType(cardEvent, createdUtc, "PLATE_NUMBER", log);
            await ProcessImageType(cardEvent, createdUtc, "VEHICLE", log);
            await _eventDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            log($"Failed to process image for entry {cardEvent.Id}: {ex.Message}");
        }
    }

    private async Task ProcessImageType(CardEvent cardEvent, DateTime createdUtc, string imageType, Action<string> log)
    {
        var objectKey = BuildImageObjectKey(createdUtc);

        var existedImage = await _eventDbContext.EntryImages
            .AnyAsync(img => img.EntryId == cardEvent.Id && img.Type == imageType);

        if (existedImage)
            return;

        var uploadSuccess = await UploadImageToMinIO(cardEvent.PicDirIn, objectKey, log);

        if (uploadSuccess)
        {
            await AddEntryImage(cardEvent.Id, objectKey, imageType);
            log($"{imageType} image uploaded: {objectKey}");
        }
        else
        {
            log($"Failed to upload {imageType} image: {cardEvent.PicDirIn}");
        }
    }

    private async Task AddEntryImage(Guid entryId, string objectKey, string imageType)
    {
        var entryImage = new EntryImage
        {
            EntryId = entryId,
            ObjectKey = objectKey,
            Type = imageType
        };
        _eventDbContext.EntryImages.Add(entryImage);
    }

    private async Task EnsureBucketExists(Action<string> log)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_minioSettings.Bucket);
            var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!bucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_minioSettings.Bucket);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                log($"Created bucket: {_minioSettings}");
            }
        }
        catch (Exception ex)
        {
            log($"Error ensuring bucket exists: {ex.Message}");
            throw;
        }
    }

    private async Task<bool> UploadImageToMinIO(string localFilePath, string objectKey, Action<string> log)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                log($"Local file not found: {localFilePath}");
                return false;
            }

            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_minioSettings.Bucket)
                    .WithObject(objectKey);
                await _minioClient.StatObjectAsync(statObjectArgs);

                log($"Object already exists in MinIO: {objectKey}");
                return true;
            }
            catch (Minio.Exceptions.ObjectNotFoundException)
            {
            }

            using var fileStream = File.OpenRead(localFilePath);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_minioSettings.Bucket)
                .WithObject(objectKey)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType("image/jpeg");

            await _minioClient.PutObjectAsync(putObjectArgs);

            log($"Successfully uploaded to MinIO: {objectKey}");
            return true;
        }
        catch (Exception ex)
        {
            log($"Error uploading to MinIO: {ex.Message}");
            return false;
        }
    }

    private string BuildImageObjectKey(DateTime createdUtc)
    {
        var dateInYyMMdd = createdUtc.ToString("yyMMdd");
        var hour = createdUtc.ToString("HH");
        var guidPart = Guid.NewGuid().ToString();

        return $"{dateInYyMMdd}/{hour}/{guidPart}.jpg";
    }

    private async Task<AccessKey?> MigrateAccessKey(CardEvent ce, Action<string> log)
    {
        log($"AccessKey: {ce.CardNumber}");

        var eventAccessKey = await _eventDbContext.AccessKeys
            .FirstOrDefaultAsync(a => a.Code.ToLower() == ce.CardNumber.ToLower());

        if (eventAccessKey != null)
            return eventAccessKey;

        var resourceAccessKey = await _resourceDbContext.AccessKeys
            .FirstOrDefaultAsync(a => a.Code.ToLower() == ce.CardNumber.ToLower());

        if (resourceAccessKey == null)
            return null;

        if (resourceAccessKey.Deleted)
        {
            log($"AccessKey {resourceAccessKey.Code} has been deleted");
            return null;
        }

        eventAccessKey = new AccessKey
        {
            Id = resourceAccessKey.Id,
            Code = resourceAccessKey.Code,
            Name = resourceAccessKey.Name,
            Type = resourceAccessKey.Type,
            CollectionId = resourceAccessKey.CollectionId,
            Status = resourceAccessKey.Status,
        };

        _eventDbContext.AccessKeys.Add(eventAccessKey);
        await _eventDbContext.SaveChangesAsync();

        return eventAccessKey;
    }

    private async Task<Device?> MigrateDevice(CardEvent ce, Action<string> log)
    {
        var lane = await _mParkingDbContext.Lanes
            .FirstOrDefaultAsync(l => l.LaneID == Guid.Parse(ce.LaneIDIn));

        if (lane == null)
            return null;

        log($"Lane: {lane.LaneName}");

        var eventDevice = await _eventDbContext.Devices
            .FirstOrDefaultAsync(d => d.Name == lane.LaneName);

        if (eventDevice != null)
            return eventDevice;

        var resourceDevice = await _resourceDbContext.Devices
            .FirstOrDefaultAsync(d => d.Name == lane.LaneName);

        if (resourceDevice == null)
            return null;

        eventDevice = new Device
        {
            Id = resourceDevice.Id,
            Name = resourceDevice.Name,
            Type = resourceDevice.Type,
        };

        _eventDbContext.Devices.Add(eventDevice);
        await _eventDbContext.SaveChangesAsync();

        return eventDevice;
    }

    private async Task<Customer?> MigrateCustomer(CardEvent ce, Action<string> log)
    {
        log($"Customer: {(string.IsNullOrEmpty(ce.CustomerName) ? "✖️" : ce.CustomerName)}");

        var eventCustomer = await _eventDbContext.Customers
            .FirstOrDefaultAsync(c => c.Name == ce.CustomerName && c.Code == ce.CustomerCode);

        if (eventCustomer != null)
            return eventCustomer;

        var resourceCustomer = await _resourceDbContext.Customers
            .FirstOrDefaultAsync(c => c.Name == ce.CustomerName && c.Code == ce.CustomerCode);

        if (resourceCustomer == null)
            return null;

        eventCustomer = new Customer
        {
            Id = resourceCustomer.Id,
            Name = resourceCustomer.Name,
            Code = resourceCustomer.Code
        };

        _eventDbContext.Customers.Add(eventCustomer);
        await _eventDbContext.SaveChangesAsync();

        return eventCustomer;
    }
}