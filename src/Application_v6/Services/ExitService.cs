using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v6;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class ExitService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task InsertExit(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        // var exits = await parkingDbContext.EventOuts.AsQueryable().AsNoTracking()
        //     .Where(e => !e.Deleted && e.CreatedUtc >= fromDate)
        //     .Include(e => e.EventOutFiles)
        //     .ThenInclude(eif => eif.File)
        //     .ToListAsync(cancellationToken: token);

        var query = parkingDbContext.EventOuts.AsNoTracking()
            .Where(e => !e.Deleted && e.CreatedUtc >= fromDate)
            .Include(e => e.EventOutFiles)
            .ThenInclude(eif => eif.File);

        var totalCount = await query.CountAsync(token);
        var batchSize = 1000;

        for (int i = 0; i < totalCount; i += batchSize)
        {
            var exits = await query
                .OrderBy(e => e.CreatedUtc)
                .Skip(i)
                .Take(batchSize)
                .ToListAsync(token);

            foreach (var eo in exits)
            {
                token.ThrowIfCancellationRequested();

                var existsExit = await eventDbContext.Exits.AnyAsync(e => e.Id == eo.Id, cancellationToken: token);
                var existsCustomer = await eventDbContext.Customers.AnyAsync(c => c.Id == eo.CustomerId, token);

                var eventIn = await parkingDbContext.EventIns
                    .Include(ei => ei.EventInFiles)
                    .ThenInclude(eif => eif.File)
                    .FirstOrDefaultAsync(ei => ei.Id == eo.EventInId, token);

                if (eventIn == null) continue;

                var existsAccessKey = await eventDbContext.AccessKeys
                    .AnyAsync(ak => ak.Id == eventIn.IdentityId, token);

                if (!existsAccessKey) continue;

                var existsEntry = await eventDbContext.Entries.AnyAsync(e => e.Id == eventIn.Id, token);

                if (!existsEntry)
                {
                    var entry = new Entry
                    {
                        Id = eventIn.Id,
                        PlateNumber = eventIn.PlateNumber,
                        DeviceId = eventIn.LaneId,
                        AccessKeyId = eventIn.IdentityId,
                        Exited = true,
                        Amount = eventIn.TotalPaid,
                        Deleted = false,
                        Note = eventIn.Note,
                        CreatedBy = eventIn.CreatedBy,
                        CreatedUtc = eventIn.CreatedUtc,
                        UpdatedUtc = eventIn.UpdatedUtc,
                        CustomerId = (eventIn.CustomerId.HasValue &&
                                      await eventDbContext.Customers.AnyAsync(c => c.Id == eventIn.CustomerId, token))
                            ? eventIn.CustomerId
                            : null
                    };

                    eventDbContext.Entries.Add(entry);
                    await InsertEntryImagesAsync(eventIn, token);
                }

                if (!existsExit)
                {
                    var exit = new Exit
                    {
                        Id = eo.Id,
                        EntryId = eo.EventInId,
                        AccessKeyId = eo.IdentityId,
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

                    eventDbContext.Exits.Add(exit);
                    await eventDbContext.SaveChangesAsync(token);

                    await InsertExitImagesAsync(eo, token);
                    await eventDbContext.SaveChangesAsync(token);

                    inserted++;
                    log($"[INSERTED] {eo.Id}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {eo.Id}");
                }

                log($"Tổng: {exits.Count}");
            }
        }


        log("========== KẾT QUẢ ==========");

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

            bool existsImage = await eventDbContext.EntryImages.AnyAsync(img =>
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

                eventDbContext.EntryImages.Add(entryImage);
            }
        }
    }

    private async Task InsertExitImagesAsync(EventOut eo, CancellationToken token)
    {
        if (eo.EventOutFiles == null || !eo.EventOutFiles.Any()) return;

        foreach (var eif in eo.EventOutFiles)
        {
            if (eif.File == null) continue;

            var newType = ConvertImageType(eif.ImageType);

            bool existsImage = await eventDbContext.ExitImages.AnyAsync(img =>
                img.ExitId == eo.Id &&
                img.ObjectKey == eif.File.ObjectKey &&
                img.Type == newType, token);

            if (!existsImage)
            {
                var exitImage = new ExitImage
                {
                    ExitId = eo.Id,
                    ObjectKey = eif.File.ObjectKey,
                    Type = newType,
                };

                eventDbContext.ExitImages.Add(exitImage);
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