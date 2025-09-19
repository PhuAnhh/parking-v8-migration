using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v6;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class EntryService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public EntryService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
    }

    public async Task InsertEntry(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;

        var entries = await _parkingDbContext.EventIns
            .Where(e => !e.Deleted && e.CreatedUtc >= fromDate && e.Status == "Parking")
            .Include(e => e.EventInFiles)
            .ThenInclude(eif => eif.File)
            .ToListAsync(token);

        foreach (var ei in entries)
        {
            token.ThrowIfCancellationRequested();

            var existsEntry = await _eventDbContext.Entries.AnyAsync(e => e.Id == ei.Id);
            var existsCustomer = await _eventDbContext.Customers.AnyAsync(c => c.Id == ei.CustomerId, token);

            if (!existsEntry)
            {
                var entry = new Entry
                {
                    Id = ei.Id,
                    AccessKeyId = ei.IdentityId,
                    DeviceId = ei.LaneId,
                    PlateNumber = ei.PlateNumber,
                    CustomerId = existsCustomer ? ei.CustomerId : null,
                    Exited = false,
                    Amount = ei.TotalPaid,
                    CreatedBy = ei.CreatedBy,
                    Note = ei.Note,
                    Deleted = ei.Deleted,
                    CreatedUtc = ei.CreatedUtc,
                    UpdatedUtc = ei.UpdatedUtc,
                };

                _eventDbContext.Entries.Add(entry);
                await InsertEntryImagesAsync(ei, token);

                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {ei.Id} đã thêm vào Event");
            }
            else
            {
                skipped++;
                log($"[SKIP] {ei.Id} đã tồn tại");
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {entries.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }

    private async Task InsertEntryImagesAsync(EventIn ei, CancellationToken token)
    {
        if (ei.EventInFiles == null || !ei.EventInFiles.Any()) return;

        foreach (var eif in ei.EventInFiles)
        {
            if (eif.File == null) continue;

            var newType = ConvertImageType(eif.ImageType);

            bool existsImage = await _eventDbContext.EntryImages.AnyAsync(img =>
                img.EntryId == ei.Id &&
                img.ObjectKey == eif.File.ObjectKey &&
                img.Type == newType, token);

            if (!existsImage)
            {
                var entryImage = new EntryImage
                {
                    EntryId = ei.Id,
                    ObjectKey = eif.File.ObjectKey,
                    Type = newType,
                };

                _eventDbContext.EntryImages.Add(entryImage);
                
                // var filePath = Path.Combine("D:/images", eif.File.ObjectKey);
                // var contentType = eif.File.ContentType ?? "image/jpeg";
                // await _minioService.UploadFileAsync(eif.File.ObjectKey, filePath, contentType, token);
            }
        }
    }

    string ConvertImageType(string? oldType)
    {
        return oldType switch
        {
            "Plate" => "PLATE_NUMBER",
            "Overview" => "PANORAMA",
            "Vehicle" => "VEHICLE",
        };
    }
}