using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v6.Parking;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

public class ExitService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task InsertExit(Action<string> log, CancellationToken token)
    {
        const int batchSize = 5000;
        int inserted = 0, skipped = 0;

        var query = parkingDbContext.EventOuts
            .AsNoTracking()
            .Where(e => !e.Deleted)
            .OrderBy(e => e.CreatedUtc)
            .Include(e => e.EventOutFiles)
            .ThenInclude(eif => eif.File);

        DateTime? lastCreatedUtc = null;

        var existingExits = new HashSet<Guid>(
            await eventDbContext.Exits
                .AsNoTracking()
                .Select(e => e.Id)
                .ToListAsync(token));

        var existingEntries = new HashSet<Guid>(
            await eventDbContext.Entries
                .AsNoTracking()
                .Select(e => e.Id)
                .ToListAsync(token));

        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(token));

        var existingAccessKeys = new HashSet<Guid>(
            await eventDbContext.AccessKeys
                .AsNoTracking()
                .Select(ak => ak.Id)
                .ToListAsync(token));

        var existingCollections = new HashSet<Guid>(
            await eventDbContext.AccessKeyCollections
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(token));

        while (true)
        {
            var exits = await query
                .Where(e => lastCreatedUtc == null || e.CreatedUtc > lastCreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (!exits.Any())
                break;

            var newEntries = new List<Entry>();
            var newExits = new List<Exit>();
            var imageQueue = new List<EventOut>();
            var entryImageQueue = new List<EventIn>();

            foreach (var eo in exits)
            {
                token.ThrowIfCancellationRequested();

                if (existingExits.Contains(eo.Id))
                {
                    skipped++;
                    log($"[SKIPPED - EXIT EXISTS] {eo.Id}");
                    continue;
                }

                var eventIn = await parkingDbContext.EventIns
                    .Include(ei => ei.EventInFiles)
                    .ThenInclude(eif => eif.File)
                    .FirstOrDefaultAsync(ei => ei.Id == eo.EventInId, token);

                if (eventIn == null)
                {
                    skipped++;
                    log($"[SKIPPED - NO EVENT IN] {eo.Id}");
                    continue;
                }

                if (!existingAccessKeys.Contains(eventIn.IdentityId))
                {
                    skipped++;
                    log($"[SKIPPED - NO ACCESS KEY] {eo.Id}");
                    continue;
                }

                if (eventIn.IdentityGroupId == null ||
                    !existingCollections.Contains(eventIn.IdentityGroupId))
                {
                    skipped++;
                    log($"[SKIPPED - NO COLLECTION] {eo.Id}");
                    continue;
                }

                bool existsCustomer =
                    eo.CustomerId.HasValue &&
                    existingCustomers.Contains(eo.CustomerId.Value);

                if (!existingEntries.Contains(eventIn.Id))
                {
                    var entry = new Entry
                    {
                        Id = eventIn.Id,
                        PlateNumber = eventIn.PlateNumber,
                        DeviceId = eventIn.LaneId,
                        AccessKeyId = eventIn.IdentityId,
                        CollectionId = eventIn.IdentityGroupId,
                        Exited = true,
                        Amount = eventIn.TotalPaid,
                        Deleted = false,
                        Note = eventIn.Note,
                        CreatedBy = eventIn.CreatedBy,
                        CreatedUtc = eventIn.CreatedUtc,
                        UpdatedUtc = eventIn.UpdatedUtc,
                        CustomerId =
                            eventIn.CustomerId.HasValue &&
                            existingCustomers.Contains(eventIn.CustomerId.Value)
                                ? eventIn.CustomerId
                                : null
                    };

                    newEntries.Add(entry);
                    entryImageQueue.Add(eventIn);
                    existingEntries.Add(entry.Id);
                }

                var exit = new Exit
                {
                    Id = eo.Id,
                    EntryId = eo.EventInId,
                    AccessKeyId = eo.IdentityId,
                    CollectionId = eventIn.IdentityGroupId,
                    DeviceId = eo.LaneId,
                    PlateNumber = eo.PlateNumber,
                    CustomerId = existsCustomer ? eo.CustomerId : null,
                    Amount = eo.Charge,
                    DiscountAmount = eo.DiscountAmount,
                    Note = eo.Note,
                    CreatedBy = eo.CreatedBy,
                    CreatedUtc = eo.CreatedUtc,
                    UpdatedUtc = eo.UpdatedUtc,
                };

                newExits.Add(exit);
                imageQueue.Add(eo);
                existingExits.Add(exit.Id);

                inserted++;
                log($"[INSERTED] {eo.Id}");
            }

            if (newEntries.Any())
                await eventDbContext.BulkInsertAsync(newEntries, cancellationToken: token);

            if (newExits.Any())
                await eventDbContext.BulkInsertAsync(newExits, cancellationToken: token);

            foreach (var eo in imageQueue)
                await InsertExitImagesAsync(eo, token);

            foreach (var ei in entryImageQueue)
                await InsertEntryImagesAsync(ei, token);

            lastCreatedUtc = exits.Last().CreatedUtc;
            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Bỏ qua: {skipped}");
    }

    private async Task InsertExitImagesAsync(EventOut eo, CancellationToken token)
    {
        if (eo.EventOutFiles == null || !eo.EventOutFiles.Any())
            return;

        var newImages = new List<ExitImage>();

        foreach (var eif in eo.EventOutFiles)
        {
            if (eif.File == null) continue;

            var newType = ConvertImageType(eif.ImageType);
            if (newType == null) continue;

            bool existsImage = await eventDbContext.ExitImages.AnyAsync(
                img =>
                    img.ExitId == eo.Id &&
                    img.ObjectKey == eif.File.ObjectKey &&
                    img.Type == newType,
                token);

            if (!existsImage)
            {
                newImages.Add(new ExitImage
                {
                    ExitId = eo.Id,
                    ObjectKey = eif.File.ObjectKey,
                    Type = newType,
                });
            }
        }

        if (newImages.Any())
            await eventDbContext.BulkInsertAsync(newImages, cancellationToken: token);
    }

    private async Task InsertEntryImagesAsync(EventIn ei, CancellationToken token)
    {
        if (ei.EventInFiles == null || !ei.EventInFiles.Any()) return;

        var newImages = new List<EntryImage>();

        foreach (var eif in ei.EventInFiles)
        {
            if (eif.File == null) continue;

            var newType = ConvertImageType(eif.ImageType);
            if (newType == null) continue;

            bool existsImage = await eventDbContext.EntryImages.AnyAsync(
                img =>
                    img.EntryId == ei.Id &&
                    img.ObjectKey == eif.File.ObjectKey &&
                    img.Type == newType,
                token);

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
            await eventDbContext.BulkInsertAsync(newImages, cancellationToken: token);
    }

    private static string ConvertImageType(string? oldType)
    {
        return oldType switch
        {
            "Plate" => "PLATE_NUMBER",
            "Overview" => "PANORAMA",
            "Vehicle" => "VEHICLE",
            _ => "UNKNOWN"
        };
    }
}