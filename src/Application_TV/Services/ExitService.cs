using Application_TV.DbContexts.v6;
using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class ExitService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task InsertExit(Action<string> log, CancellationToken token)
    {
        var batchSize = 10000;
        int inserted = 0, skipped = 0;

        eventDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        eventDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        log("Đang load customers và collections...");
        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(token));

        var existingCollections = new HashSet<Guid>(
            await eventDbContext.AccessKeyCollections
                .AsNoTracking()
                .Select(ac => ac.Id)
                .ToListAsync(token));

        log("Đang load file map...");
        var fileMap = await parkingDbContext.PhysicalFiles
            .AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => f.FileKey, token);
        log($"Đã load {fileMap.Count} files");

        var bulkConfig = new BulkConfig
        {
            BatchSize = 5000,
            BulkCopyTimeout = 0,
            EnableStreaming = true,
            UseTempDB = true
        };

        DateTime? lastCreatedUtc = null;
        Guid? lastId = null;

        while (true)
        {
            token.ThrowIfCancellationRequested();

            var query = parkingDbContext.EventOuts.AsNoTracking();

            if (lastCreatedUtc.HasValue)
            {
                var lc = lastCreatedUtc.Value;
                var lid = lastId!.Value;

                query = query.Where(e =>
                    e.CreatedUtc > lc ||
                    (e.CreatedUtc == lc && e.Id.CompareTo(lid) > 0));
            }

            var exits = await query
                .OrderBy(e => e.CreatedUtc)
                .ThenBy(e => e.Id)
                .Take(batchSize)
                .ToListAsync(token);

            if (exits.Count == 0) break;

            var batchExitIds = exits.Select(x => x.Id).ToList();

            var existingBatchExits = new HashSet<Guid>(
                await eventDbContext.Exits
                    .AsNoTracking()
                    .Where(x => batchExitIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(token));

            var batchAccessKeyIds = exits
                .SelectMany(e => new[] { e.EventInIdentityId, e.IdentityId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var existingBatchAccessKeys = new HashSet<Guid>(
                await eventDbContext.AccessKeys
                    .AsNoTracking()
                    .Where(ak => batchAccessKeyIds.Contains(ak.Id))
                    .Select(ak => ak.Id)
                    .ToListAsync(token));

            var newEntries = new List<Entry>();
            var newExits = new List<Exit>();
            var newEntryImages = new List<EntryImage>();
            var newExitImages = new List<ExitImage>();

            foreach (var eo in exits)
            {
                token.ThrowIfCancellationRequested();

                if (existingBatchExits.Contains(eo.Id))
                {
                    skipped++;
                    continue;
                }

                if (!eo.EventInIdentityId.HasValue ||
                    !existingBatchAccessKeys.Contains(eo.EventInIdentityId.Value))
                {
                    skipped++;
                    log($"[SKIPPED - MISSING ENTRY ACCESSKEY] {eo.Id}");
                    continue;
                }

                if (!eo.IdentityId.HasValue ||
                    !existingBatchAccessKeys.Contains(eo.IdentityId.Value))
                {
                    skipped++;
                    log($"[SKIPPED - MISSING EXIT ACCESSKEY] {eo.Id}");
                    continue;
                }

                var collectionId =
                    eo.EventInIdentityGroupId.HasValue && eo.EventInIdentityGroupId.Value != Guid.Empty
                        ? eo.EventInIdentityGroupId.Value
                        : (eo.IdentityGroupId.HasValue && eo.IdentityGroupId.Value != Guid.Empty
                            ? eo.IdentityGroupId.Value
                            : Guid.Empty);

                if (collectionId == Guid.Empty || !existingCollections.Contains(collectionId))
                {
                    skipped++;
                    log($"[SKIPPED - INVALID COLLECTION] {eo.Id}");
                    continue;
                }

                var newEntryId = Guid.NewGuid();

                var entry = new Entry
                {
                    Id = newEntryId,
                    AccessKeyId = eo.EventInIdentityId.Value,
                    DeviceId = eo.EventInLaneId,
                    PlateNumber = eo.EventInPlateNumber,
                    CustomerId = (eo.CustomerId.HasValue && existingCustomers.Contains(eo.CustomerId.Value))
                        ? eo.CustomerId
                        : null,
                    Amount = eo.Charge,
                    Exited = true,
                    CollectionId = collectionId,
                    CreatedBy = "admin",
                    CreatedUtc = eo.EventInCreatedUtc,
                    UpdatedUtc = eo.UpdatedUtc ?? DateTime.UtcNow,
                    Deleted = false
                };

                var exit = new Exit
                {
                    Id = eo.Id,
                    EntryId = newEntryId,
                    AccessKeyId = eo.IdentityId.Value,
                    DeviceId = eo.LaneId,
                    PlateNumber = eo.PlateNumber,
                    CustomerId = (eo.CustomerId.HasValue && existingCustomers.Contains(eo.CustomerId.Value))
                        ? eo.CustomerId
                        : null,
                    Amount = eo.Charge,
                    DiscountAmount = eo.Discount,
                    Note = null,
                    CreatedBy = "admin",
                    CreatedUtc = eo.CreatedUtc,
                    UpdatedUtc = eo.UpdatedUtc ?? DateTime.UtcNow,
                    CollectionId = collectionId
                };

                newEntries.Add(entry);
                newExits.Add(exit);

                AddEntryImages(newEntryImages, eo.EventInPhysicalFileIds, fileMap, newEntryId);
                AddExitImages(newExitImages, eo.PhysicalFileIds, fileMap, eo.Id);

                inserted++;
            }

            if (newEntries.Any())
                await eventDbContext.BulkInsertAsync(newEntries, bulkConfig, cancellationToken: token);

            if (newExits.Any())
                await eventDbContext.BulkInsertAsync(newExits, bulkConfig, cancellationToken: token);

            if (newEntryImages.Any())
                await eventDbContext.BulkInsertAsync(newEntryImages, bulkConfig, cancellationToken: token);

            if (newExitImages.Any())
                await eventDbContext.BulkInsertAsync(newExitImages, bulkConfig, cancellationToken: token);

            eventDbContext.ChangeTracker.Clear();

            var last = exits[^1];
            lastCreatedUtc = last.CreatedUtc;
            lastId = last.Id;

            log($"Đã xử lý: {inserted + skipped} | Thành công: {inserted} | Bỏ qua: {skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Bỏ qua: {skipped}");
    }

    private static List<int> ParseImageIds(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids)) return [];

        return ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x.Trim()))
            .ToList();
    }

    private void AddEntryImages(List<EntryImage> list, string? ids, Dictionary<int, string> map, Guid entryId)
    {
        if (string.IsNullOrEmpty(ids)) return;

        var idList = ParseImageIds(ids).Where(map.ContainsKey).ToList();

        foreach (var fid in idList)
        {
            var key = map[fid];
            var type = ConvertType(key);

            if (list.Any(x => x.EntryId == entryId && x.ObjectKey == key))
                continue;

            list.Add(new EntryImage
            {
                EntryId = entryId,
                ObjectKey = key,
                Type = type
            });
        }
    }

    private void AddExitImages(List<ExitImage> list, string? ids, Dictionary<int, string> map, Guid exitId)
    {
        if (string.IsNullOrEmpty(ids)) return;

        var idList = ParseImageIds(ids).Where(map.ContainsKey).ToList();

        foreach (var fid in idList)
        {
            var key = map[fid];
            var type = ConvertType(key);

            if (list.Any(x => x.ExitId == exitId && x.ObjectKey == key))
                continue;

            list.Add(new ExitImage
            {
                ExitId = exitId,
                ObjectKey = key,
                Type = type
            });
        }
    }

    private static string ConvertType(string fileKey)
    {
        fileKey = fileKey.ToUpperInvariant();

        if (fileKey.Contains("OVERVIEW")) return "PANORAMA";
        if (fileKey.Contains("VEHICLE")) return "VEHICLE";
        if (fileKey.Contains("LPR")) return "PLATE_NUMBER";

        return "UNKNOWN";
    }
}