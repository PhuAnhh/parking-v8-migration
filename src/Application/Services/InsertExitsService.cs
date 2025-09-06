using System.Globalization;
using Application.DbContexts.v3;
using Application.DbContexts.v8;
using Application.Entities.v3;
using Application.Entities.v8;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

namespace Application.Services;

public class InsertExitsService
{
    private readonly MParkingDbContext _mParkingDbContext;
    private readonly MParkingEventDbContext _mParkingEventDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly MinioSettings _minioSettings;
    private readonly IMinioClient _minioClient;

    public InsertExitsService(
        MParkingEventDbContext mParkingEventDbContext,
        EventDbContext eventDbContext,
        MParkingDbContext mParkingDbContext,
        MinioSettings minioSettings)
    {
        _mParkingDbContext = mParkingDbContext;
        _mParkingEventDbContext = mParkingEventDbContext;
        _eventDbContext = eventDbContext;
        _minioSettings = minioSettings;

        _minioClient = new MinioClient()
            .WithEndpoint(_minioSettings.Endpoint)
            .WithCredentials(_minioSettings.AccessKey, _minioSettings.SecretKey)
            .Build();
    }

    public async Task InsertExits(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        log($"Successfully connected to MinIO bucket [{_minioSettings.Bucket}]!");

        int insertedExits = 0;
        int skippedExits = 0;

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

        var exitCardEvents = await _mParkingEventDbContext.ExitCardEventDtos
            .Where(e => e.EventCode == "2" && e.DateTimeOut >= fromDate)
            .ToListAsync();

        var cardEvents = (from c in cards
            join e in exitCardEvents on c.CardNumber equals e.CardNumber
            join cg in cardGroups on c.CardGroupID equals cg.CardGroupID into cgGroup
            from cg in cgGroup.DefaultIfEmpty()
            select new CardEvent
            {
                Id = e.Id,
                CardNumber = e.CardNumber,
                PlateOut = e.PlateOut,
                EventCode = e.EventCode,
                DateTimeOut = e.DateTimeOut,
                PicDirOut = e.PicDirOut,
                LaneIDOut = e.LaneIDOut,
                UserIDOut = e.UserIDOut,
                CustomerName = e.CustomerName,
                Moneys = e.Moneys,
                IsDelete = e.IsDelete,
                ReducedMoney = e.ReducedMoney
            }).ToList();

        foreach (var ce in cardEvents)
        {
            token.ThrowIfCancellationRequested();

            var entry = await _eventDbContext.Entries
                .Include(en => en.AccessKey)
                .Where(en => en.AccessKey.Code == ce.CardNumber && !en.Exited)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                log($"No entry event found for card {ce.CardNumber}");
                continue;
            }

            var existedExit = await _eventDbContext.Exits.AnyAsync(x => x.Id == ce.Id);
            if (!existedExit)
            {
                var exit = new Exit
                {
                    Id = ce.Id,
                    EntryId = entry.Id,
                    AccessKeyId = entry.AccessKeyId,
                    PlateNumber = ce.PlateOut,
                    DeviceId = Guid.Parse(ce.LaneIDOut),
                    Amount = (long)ce.Moneys,
                    DiscountAmount = (long)ce.ReducedMoney,
                    Deleted = false,
                    CreatedBy = "admin",
                    CreatedUtc = TimeZoneInfo.ConvertTimeToUtc(ce.DateTimeOut,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                    CustomerId = entry?.CustomerId
                };

                _eventDbContext.Exits.Add(exit);
                entry.Exited = true;

                await _eventDbContext.SaveChangesAsync();

                insertedExits++;
            }
            else
            {
                skippedExits++;
            }

            if (!string.IsNullOrEmpty(ce.PicDirOut))
            {
                await ProcessExitImages(ce, log);
            }
        }

        log($"---------- Processing Result ----------");
        log($"Total number of events: {cardEvents.Count}");
        log($"Events successfully migrated: {insertedExits}");
        log($"Events already existed (skipped): {skippedExits}");
    }

    private async Task ProcessExitImages(CardEvent cardEvent, Action<string> log)
    {
        try
        {
            await ProcessExitImageType(cardEvent, "PLATE_NUMBER", log);
            await ProcessExitImageType(cardEvent, "VEHICLE", log);
            await _eventDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            log($"Failed to process exit image for exit {cardEvent.Id}: {ex.Message}");
        }
    }

    private async Task ProcessExitImageType(CardEvent cardEvent, string imageType, Action<string> log)
    {
        var objectKey = BuildImageObjectKey(cardEvent.PicDirOut);

        var existedImage = await _eventDbContext.ExitImages
            .AnyAsync(img => img.ExitId == cardEvent.Id && img.Type == imageType);

        if (existedImage)
            return;

        var uploadSuccess = await UploadImageToMinIO(cardEvent.PicDirOut, objectKey, log);

        if (uploadSuccess)
        {
            await AddExitImage(cardEvent.Id, objectKey, imageType);
            log($"{imageType} exit image uploaded: {objectKey}");
        }
        else
        {
            log($"Failed to upload {imageType} exit image: {cardEvent.PicDirOut}");
        }
    }

    private async Task AddExitImage(Guid exitId, string objectKey, string imageType)
    {
        var exitImage = new ExitImage
        {
            ExitId = exitId,
            ObjectKey = objectKey,
            Type = imageType
        };
        _eventDbContext.ExitImages.Add(exitImage);
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
                // File doesn't exist, proceed with upload
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

    private string BuildImageObjectKey(string picDirOut)
    {
        var pathParts = picDirOut.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        var fileName = Path.GetFileName(picDirOut);
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

        if (DateTime.TryParseExact(dateFolder, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime parsedDate))
        {
            return parsedDate.ToString("yyMMdd");
        }

        return DateTime.Now.ToString("yyMMdd");
    }

    private string ExtractHourFromFileName(string fileName)
    {
        return fileName.Length >= 2 ? fileName.Substring(0, 2) : "00";
    }
}