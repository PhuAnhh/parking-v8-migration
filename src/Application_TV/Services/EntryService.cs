using Application_TV.DbContexts.v6;
using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class EntryService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task InsertEntry(Action<string> log, CancellationToken token)
    {
        var batchSize = 5000;
        int inserted = 0, skipped = 0;

        var query = parkingDbContext.EventIns.AsNoTracking()
            .Where(e => e.Status == "Parking")
            .OrderBy(e => e.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var existingEntries = new HashSet<Guid>(
            await eventDbContext.Entries.AsNoTracking().Select(e => e.Id).ToListAsync(token));

        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var existingAccessKeys = new HashSet<Guid>(
            await eventDbContext.AccessKeys.AsNoTracking().Select(ak => ak.Id).ToListAsync(token));

        // Load PhysicalFile map
        var fileMap = await parkingDbContext.PhysicalFiles
            .AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => f.FileKey, token);

        while (true)
        {
            var entries = await query
                .Where(e => lastCreatedUtc == null || e.CreatedUtc > lastCreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (entries.Count == 0)
                break;

            var newEntries = new List<Entry>();
            var newImages = new List<EntryImage>();

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
                        CollectionId = ei.IdentityGroupId,
                        Exited = false,
                        Amount = ei.Paid,
                        CreatedBy = "admin",
                        Note = ei.Note,
                        CreatedUtc = ei.CreatedUtc,
                        UpdatedUtc = ei.UpdatedUtc ?? DateTime.UtcNow,
                    };

                    newEntries.Add(entry);
                    existingEntries.Add(entry.Id);

                    inserted++;
                    log($"[INSERTED] {ei.Id}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {ei.Id}");
                }

                if (!string.IsNullOrEmpty(ei.PhysicalFileIds))
                {
                    var ids = ei.PhysicalFileIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x.Trim()))
                        .Where(id => fileMap.ContainsKey(id))
                        .ToList();

                    foreach (var id in ids)
                    {
                        string fileKey = fileMap[id];
                        string imgType = ConvertType(fileKey);

                        newImages.Add(new EntryImage
                        {
                            EntryId = ei.Id,
                            ObjectKey = fileKey,
                            Type = imgType
                        });
                    }
                }
            }

            if (newEntries.Any())
                await eventDbContext.BulkInsertAsync(newEntries, cancellationToken: token);

            if (newImages.Any())
                await eventDbContext.BulkInsertAsync(newImages, cancellationToken: token);

            lastCreatedUtc = entries.Last().CreatedUtc;
            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }

    private static string ConvertType(string fileKey)
    {
        if (fileKey.Contains("OVERVIEWIN", StringComparison.OrdinalIgnoreCase))
            return "PANORAMA";
        if (fileKey.Contains("VEHICLEIN", StringComparison.OrdinalIgnoreCase))
            return "VEHICLE";
        if (fileKey.Contains("LPRIN", StringComparison.OrdinalIgnoreCase))
            return "PLATE_NUMBER";

        return "UNKNOWN";
    }
}
