using Application_TV.DbContexts.v6;
using Application_TV.DbContexts.v8;
using Application_TV.Entities.v6;
using Application_TV.Entities.v8;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class ExitService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task InsertExit(Action<string> log, CancellationToken token)
    {
        var batchSize = 5000;
        int inserted = 0, skipped = 0;

        var query = parkingDbContext.EventOuts.AsNoTracking()
            .OrderBy(e => e.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var existingExits = new HashSet<Guid>(
            await eventDbContext.Exits.AsNoTracking().Select(e => e.Id).ToListAsync(token));

        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var existingAccessKeys = new HashSet<Guid>(
            await eventDbContext.AccessKeys.AsNoTracking().Select(ak => ak.Id).ToListAsync(token));

        var existingCollections = new HashSet<Guid>(
            await eventDbContext.AccessKeyCollections.AsNoTracking().Select(ac => ac.Id).ToListAsync(token));

        var fileMap = await parkingDbContext.PhysicalFiles
            .AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => f.FileKey, token);

        while (true)
        {
            var exits = await query
                .Where(e => lastCreatedUtc == null || e.CreatedUtc > lastCreatedUtc)
                .OrderBy(e => e.CreatedUtc)
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
                    log($"[SKIPPED - EXIT EXISTS] {eo.Id}");
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
                log($"[INSERTED EXIT + ENTRY] {eo.Id} -> Entry {newEntryId}");
            }

            if (newEntries.Any())
                await eventDbContext.BulkInsertAsync(newEntries, cancellationToken: token);

            if (newExits.Any())
                await eventDbContext.BulkInsertAsync(newExits, cancellationToken: token);

            if (newEntryImages.Any()) await eventDbContext.BulkInsertAsync(newEntryImages, cancellationToken: token);
            if (newExitImages.Any()) await eventDbContext.BulkInsertAsync(newExitImages, cancellationToken: token);

            // foreach (var eo in imageQueue)
            //     await InsertExitImagesAsync(eo, token);
            //
            // foreach (var ei in entryImageQueue)
            //     await InsertEntryImagesAsync(ei, token);

            lastCreatedUtc = exits.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }

    private static void AddEntryImages(
        List<EntryImage> list,
        string? ids,
        Dictionary<int, string> map,
        Guid entryId)
    {
        if (string.IsNullOrWhiteSpace(ids)) return;

        var idList = ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x.Trim()))
            .Where(map.ContainsKey)
            .ToList();

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

    private static void AddExitImages(
        List<ExitImage> list,
        string? ids,
        Dictionary<int, string> map,
        Guid exitId)
    {
        if (string.IsNullOrWhiteSpace(ids)) return;

        var idList = ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x.Trim()))
            .Where(map.ContainsKey)
            .ToList();

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
    // private async Task InsertExitImagesAsync(EventOut eo, CancellationToken token)
    // {
    //     if (eo.EventOutFiles == null || !eo.EventOutFiles.Any()) return;
    //
    //     var newImages = new List<ExitImage>();
    //
    //     foreach (var eif in eo.EventOutFiles)
    //     {
    //         if (eif.File == null) continue;
    //
    //         var newType = ConvertImageType(eif.ImageType);
    //         bool existsImage = await eventDbContext.ExitImages.AnyAsync(img =>
    //             img.ExitId == eo.Id &&
    //             img.ObjectKey == eif.File.ObjectKey &&
    //             img.Type == newType, token);
    //
    //         if (!existsImage)
    //         {
    //             newImages.Add(new ExitImage
    //             {
    //                 ExitId = eo.Id,
    //                 ObjectKey = eif.File.ObjectKey,
    //                 Type = newType,
    //             });
    //         }
    //     }
    //
    //     if (newImages.Any())
    //         await eventDbContext.BulkInsertAsync(newImages, cancellationToken: token);
    // }

    // private async Task InsertEntryImagesAsync(EventIn ei, CancellationToken token)
    // {
    //     if (ei.EventInFiles == null || !ei.EventInFiles.Any()) return;
    //
    //     var newImages = new List<EntryImage>();
    //
    //     foreach (var eif in ei.EventInFiles)
    //     {
    //         if (eif.File == null) continue;
    //
    //         var newType = ConvertImageType(eif.ImageType);
    //         bool existsImage = await eventDbContext.EntryImages.AnyAsync(img =>
    //             img.EntryId == ei.Id &&
    //             img.ObjectKey == eif.File.ObjectKey &&
    //             img.Type == newType, token);
    //
    //         if (!existsImage)
    //         {
    //             newImages.Add(new EntryImage
    //             {
    //                 EntryId = ei.Id,
    //                 ObjectKey = eif.File.ObjectKey,
    //                 Type = newType,
    //             });
    //         }
    //     }
    //
    //     if (newImages.Any())
    //         await eventDbContext.BulkInsertAsync(newImages, cancellationToken: token);
    // }

    // private static string ConvertImageType(string? oldType)
    // {
    //     return oldType switch
    //     {
    //         "Plate" => "PLATE_NUMBER",
    //         "Overview" => "PANORAMA",
    //         "Vehicle" => "VEHICLE",
    //         _ => "UNKNOWN"
    //     };
    // }
}