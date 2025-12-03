using Application_TV.DbContexts.v6;
using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application_TV.Services;

public class ExitService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    private readonly ConcurrentDictionary<string, List<int>> _parsedImageIds = new();

    public async Task InsertExit(Action<string> log, CancellationToken token)
    {
        var batchSize = 10000;
        int inserted = 0, skipped = 0;

        eventDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        eventDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        log("Đang load danh sách exits đã tồn tại...");
        var existingExits = await LoadExistingExitsInBatches(token);
        log($"Đã load {existingExits.Count} exits tồn tại");

        log("Đang load customers và collections...");
        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var existingCollections = new HashSet<Guid>(
            await eventDbContext.AccessKeyCollections.AsNoTracking().Select(ac => ac.Id).ToListAsync(token));

        log("Đang load access keys...");
        var existingAccessKeys = await LoadRelevantAccessKeys(token);
        log($"Đã load {existingAccessKeys.Count} access keys");

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

        while (true)
        {
            var exits = await parkingDbContext.EventOuts
                .AsNoTracking()
                .Where(e => lastCreatedUtc == null || e.CreatedUtc > lastCreatedUtc)
                .OrderBy(e => e.CreatedUtc)
                .ThenBy(e => e.Id)
                .Take(batchSize)
                .ToListAsync(token);

            if (exits.Count == 0) break;

            var newEntries = new List<Entry>();
            var newExits = new List<Exit>();
            var newEntryImages = new List<EntryImage>();
            var newExitImages = new List<ExitImage>();

            foreach (var eo in exits)
            {
                token.ThrowIfCancellationRequested();

                if (existingExits.Contains(eo.Id))
                {
                    skipped++;
                    continue;
                }

                if (!eo.EventInIdentityId.HasValue || !existingAccessKeys.Contains(eo.EventInIdentityId.Value))
                {
                    skipped++;
                    log($"[SKIPPED - MISSING ENTRY ACCESSKEY] {eo.Id}");
                    continue;
                }

                if (!eo.IdentityId.HasValue || !existingAccessKeys.Contains(eo.IdentityId.Value))
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

                existingExits.Add(exit.Id);

                AddExitImages(newExitImages, eo.PhysicalFileIds, fileMap, eo.Id);
                AddEntryImages(newEntryImages, eo.EventInPhysicalFileIds, fileMap, newEntryId);

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

            lastCreatedUtc = exits.Last().CreatedUtc;

            log($"Đã xử lý: {inserted + skipped} | Thành công: {inserted} | Bỏ qua: {skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }

    private async Task<HashSet<Guid>> LoadExistingExitsInBatches(CancellationToken token)
    {
        var existingExitIds = new HashSet<Guid>();
        var exitBatchSize = 100000;
        var skipCount = 0;

        while (true)
        {
            var batch = await eventDbContext.Exits
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .Skip(skipCount)
                .Take(exitBatchSize)
                .Select(e => e.Id)
                .ToListAsync(token);

            if (!batch.Any()) break;

            foreach (var id in batch)
                existingExitIds.Add(id);

            skipCount += exitBatchSize;
        }

        return existingExitIds;
    }

    private async Task<HashSet<Guid>> LoadRelevantAccessKeys(CancellationToken token)
    {
        var relevantAccessKeyIds = await parkingDbContext.EventOuts
            .AsNoTracking()
            .Where(e => e.EventInIdentityId.HasValue || e.IdentityId.HasValue)
            .Select(e => new { e.EventInIdentityId, e.IdentityId })
            .ToListAsync(token);

        var accessKeyIdsToCheck = relevantAccessKeyIds
            .SelectMany(x => new[] { x.EventInIdentityId, x.IdentityId })
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToList();

        return new HashSet<Guid>(
            await eventDbContext.AccessKeys
                .AsNoTracking()
                .Where(ak => accessKeyIdsToCheck.Contains(ak.Id))
                .Select(ak => ak.Id)
                .ToListAsync(token));
    }

    private List<int> ParseImageIds(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids)) return new List<int>();

        return _parsedImageIds.GetOrAdd(ids, key =>
            key.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x.Trim()))
                .ToList()
        );
    }

    private void AddEntryImages(List<EntryImage> list, string? ids, Dictionary<int, string> map, Guid entryId)
    {
        if (string.IsNullOrEmpty(ids)) return;

        var idList = ParseImageIds(ids).Where(map.ContainsKey).ToList();

        foreach (var fid in idList)
        {
            var key = map[fid];
            var type = ConvertType(key);

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