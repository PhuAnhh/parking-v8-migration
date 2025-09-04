using System.Globalization;
using Application.DbContexts.v3;
using Application.DbContexts.v8;
using Application.Entities.v3;
using Application.Entities.v3.Models;
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
    private readonly IMinioClient _minioClient;

    private const string BucketName = "image";
    private const string MinioEndpoint = "192.168.21.100:9000";
    private const string MinioAccessKey = "admin";
    private const string MinioSecretKey = "Pass1234!";

    public InsertEntriesService(MParkingEventDbContext mParkingEventDbContext, EventDbContext eventDbContext,
        ResourceDbContext resourceDbContext, MParkingDbContext mParkingDbContext, IMinioClient minioClient = null)
    {
        _mParkingEventDbContext = mParkingEventDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
        _mParkingDbContext = mParkingDbContext;

        _minioClient = minioClient ?? new MinioClient()
            .WithEndpoint(MinioEndpoint)
            .WithCredentials(MinioAccessKey, MinioSecretKey)
            .Build();
    }

    public async Task InsertEntries(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int insertedEntries = 0;
        int skippedEntries = 0;

        // Ensure bucket exists
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
            if (accessKey == null) continue;

            log($"Customer: {ce.CustomerName}");
            var eventCustomer = await _eventDbContext.Customers.FirstOrDefaultAsync(c => c.Name == ce.CustomerName);
            if (eventCustomer == null)
            {
                var resourceCustomer =
                    await _resourceDbContext.Customers.FirstOrDefaultAsync(c => c.Name == ce.CustomerName);
                if (resourceCustomer != null)
                {
                    eventCustomer = new Customer
                    {
                        Id = resourceCustomer.Id,
                        Name = resourceCustomer.Name,
                        Code = resourceCustomer.Code
                    };
                    _eventDbContext.Customers.Add(eventCustomer);
                    await _eventDbContext.SaveChangesAsync();
                }
            }

            log($"Lane: {ce.LaneIDIn}");
            var eventDevice = await _eventDbContext.Devices.FirstOrDefaultAsync(d => d.Id == Guid.Parse(ce.LaneIDIn));
            if (eventDevice == null)
            {
                var resourceDevice =
                    await _resourceDbContext.Devices.FirstOrDefaultAsync(d => d.Id == Guid.Parse(ce.LaneIDIn));
                if (resourceDevice != null)
                {
                    eventDevice = new Device
                    {
                        Id = resourceDevice.Id,
                        Name = resourceDevice.Name,
                        Type = resourceDevice.Type,
                    };
                    _eventDbContext.Devices.Add(eventDevice);
                    await _eventDbContext.SaveChangesAsync();
                }
            }

            var existedEntry = await _eventDbContext.Entries.AnyAsync(e => e.Id == ce.Id);
            if (!existedEntry)
            {
                var entry = new Entry
                {
                    Id = ce.Id,
                    PlateNumber = ce.PlateIn,
                    DeviceId = eventDevice.Id,
                    AccessKeyId = accessKey.Id,
                    Exited = false,
                    Amount = (long)ce.Moneys,
                    Deleted = false,
                    CreatedBy = "admin",
                    CreatedUtc = DateTime.SpecifyKind(ce.DatetimeIn, DateTimeKind.Utc),
                    CustomerId = eventCustomer?.Id,
                };
                _eventDbContext.Entries.Add(entry);
                await _eventDbContext.SaveChangesAsync();

                insertedEntries++;

                var localFile =
                    @"\\192.168.21.88\pic\20-08-2025\06h26m27s_de7de524-bfe8-49dc-af38-ebc5211eb434PLATEIN.JPG";
                var objectKey = "250825/06/de7de524-bfe8-49dc-af38-ebc5211eb434.jpg";
                bool success = await UploadImageToMinIO(localFile, objectKey, Console.WriteLine);
                Console.WriteLine(success ? "Upload thành công" : "Upload thất bại");
            }

            if (!string.IsNullOrEmpty(ce.PicDirIn))
            {
                try
                {
                    var plateObjectKey = BuildImageObjectKey(ce.PicDirIn);
                    var vehicleObjectKey = BuildImageObjectKey(ce.PicDirIn);

                    var existedPlateImage = await _eventDbContext.EntryImages
                        .AnyAsync(img => img.EntryId == ce.Id && img.Type == "PLATE_NUMBER");

                    if (!existedPlateImage)
                    {
                        var plateUploadSuccess = await UploadImageToMinIO(ce.PicDirIn, plateObjectKey, log);

                        if (plateUploadSuccess)
                        {
                            var entryImagePlate = new EntryImage
                            {
                                EntryId = ce.Id,
                                ObjectKey = plateObjectKey,
                                Type = "PLATE_NUMBER"
                            };
                            _eventDbContext.EntryImages.Add(entryImagePlate);
                            log($"Plate image uploaded: {plateObjectKey}");
                        }
                        else
                        {
                            log($"Failed to upload plate image: {ce.PicDirIn}");
                        }
                    }

                    var existedVehicleImage = await _eventDbContext.EntryImages
                        .AnyAsync(img => img.EntryId == ce.Id && img.Type == "VEHICLE");

                    if (!existedVehicleImage)
                    {
                        var vehicleUploadSuccess = await UploadImageToMinIO(ce.PicDirIn, vehicleObjectKey, log);

                        if (vehicleUploadSuccess)
                        {
                            var entryImageVehicle = new EntryImage
                            {
                                EntryId = ce.Id,
                                ObjectKey = vehicleObjectKey,
                                Type = "VEHICLE"
                            };
                            _eventDbContext.EntryImages.Add(entryImageVehicle);
                            log($"Vehicle image uploaded: {vehicleObjectKey}");
                        }
                        else
                        {
                            log($"Failed to upload vehicle image: {ce.PicDirIn}");
                        }
                    }

                    await _eventDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    log($"Lỗi xử lý ảnh cho entry {ce.Id}: {ex.Message}");
                }
            }
            else
            {
                skippedEntries++;
            }
        }

        log("---------- Kết quả ----------");
        log($"Tổng {cardEvents.Count} sự kiện");
        log($"Đã di chuyển {insertedEntries} sự kiện");
        log($"Tồn tại {skippedEntries} sự kiện");
    }

    private async Task EnsureBucketExists(Action<string> log)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(BucketName);
            var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!bucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(BucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                log($"Created bucket: {BucketName}");
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
                    .WithBucket(BucketName)
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
                .WithBucket(BucketName)
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

    private string BuildImageObjectKey(string picDirIn)
    {
        var pathParts = picDirIn.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        var fileName = Path.GetFileName(picDirIn);

        var dateFolder = pathParts.Length >= 3 ? pathParts[^2] : null;

        var dateInYyMMdd = ConvertDateToYyMMdd(dateFolder);

        var hour = ExtractHourFromFileName(fileName);

        var guidPart = Guid.NewGuid().ToString();

        return $"{dateInYyMMdd}/{hour}/{guidPart}.jpg";
    }

    private string ConvertDateToYyMMdd(string dateFolder)
    {
        if (string.IsNullOrEmpty(dateFolder))
            return DateTime.Now.ToString("yyMMdd");

        if (DateTime.TryParseExact(dateFolder, "dd-MM-yyyy", null,
                DateTimeStyles.None, out DateTime parsedDate))
        {
            return parsedDate.ToString("yyMMdd");
        }

        return DateTime.Now.ToString("yyMMdd");
    }

    private string ExtractHourFromFileName(string fileName)
    {
        return fileName.Length >= 2 ? fileName.Substring(0, 2) : "00";
    }

    private async Task<AccessKey?> MigrateAccessKey(CardEvent ce, Action<string> log)
    {
        log($"AccessKey: {ce.CardNumber}");

        var eventAccessKey = await _eventDbContext.AccessKeys.FirstOrDefaultAsync(a =>
             a.Code.ToLower() == ce.CardNumber.ToLower());
        if (eventAccessKey != null) return eventAccessKey;

        var resourceAccessKey = await _resourceDbContext.AccessKeys.FirstOrDefaultAsync(a =>
             a.Code.ToLower() == ce.CardNumber.ToLower());
        if (resourceAccessKey == null) return null;

        if (resourceAccessKey.Deleted)
        {
            log($"AccessKey {resourceAccessKey.Code} đã bị xóa");
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
}