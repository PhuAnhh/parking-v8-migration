using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v6.Parking;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

public class EntryService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task InsertEntry(Action<string> log, CancellationToken token)
    {
        var batchSize = 5000;
        int inserted = 0, skipped = 0;

        var query = parkingDbContext.EventIns.AsNoTracking()
            .Where(e => !e.Deleted && e.Status == "Parking")
            .OrderBy(e => e.CreatedUtc)
            .Include(e => e.EventInFiles)
            .ThenInclude(eif => eif.File);

        DateTime? lastCreatedUtc = null;

        var existingEntries = new HashSet<Guid>(
            await eventDbContext.Entries.AsNoTracking().Select(e => e.Id).ToListAsync(token));

        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var existingAccessKeys = new HashSet<Guid>(
            await eventDbContext.AccessKeys.AsNoTracking().Select(ak => ak.Id).ToListAsync(token));

        while (true)
        {
            var entries = await query
                .Where(e => lastCreatedUtc == null || e.CreatedUtc > lastCreatedUtc)
                .OrderBy(e => e.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (entries.Count == 0)
                break;

            var newEntries = new List<Entry>();
            var imageQueue = new List<EventIn>();

            foreach (var ei in entries)
            {
                token.ThrowIfCancellationRequested();

                bool existsEntry = existingEntries.Contains(ei.Id);
                bool existsCustomer = ei.CustomerId.HasValue && existingCustomers.Contains(ei.CustomerId.Value);

                if (!existingAccessKeys.Contains(ei.IdentityId))
                    continue;

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

                    newEntries.Add(entry);
                    imageQueue.Add(ei);

                    existingEntries.Add(entry.Id);

                    inserted++;
                    log($"[INSERTED] {ei.Id}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {ei.Id}");
                }
            }

            if (newEntries.Any())
                await eventDbContext.BulkInsertAsync(newEntries, cancellationToken: token);

            foreach (var ei in imageQueue)
                await InsertEntryImagesAsync(ei, token);

            lastCreatedUtc = entries.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }

    private async Task InsertEntryImagesAsync(EventIn ei, CancellationToken token)
    {
        if (ei.EventInFiles == null || !ei.EventInFiles.Any())
            return;

        var newImages = new List<EntryImage>();

        foreach (var eif in ei.EventInFiles)
        {
            if (eif.File == null)
                continue;

            var newType = ConvertImageType(eif.ImageType);

            bool existsImage = await eventDbContext.EntryImages.AnyAsync(img =>
                img.EntryId == ei.Id &&
                img.ObjectKey == eif.File.ObjectKey &&
                img.Type == newType, token);

            if (!existsImage)
            {
                newImages.Add(new EntryImage
                {
                    EntryId = ei.Id,
                    ObjectKey = eif.File.ObjectKey,
                    Type = newType,
                });
            }
        }

        if (newImages.Any())
        {
            await eventDbContext.BulkInsertAsync(newImages, cancellationToken: token);
        }
    }

    private static string ConvertImageType(string? oldType)
    {
        return oldType switch
        {
            "Plate" => "PLATE_NUMBER",
            "Overview" => "PANORAMA",
            "Vehicle" => "VEHICLE",
        };
    }
}