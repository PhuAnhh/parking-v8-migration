using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_v6.Services;

public class ExitImageService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
{
    public async Task Insert(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        var batchSize = 5000;
        int inserted = 0, skipped = 0;

        var query = parkingDbContext.EventOutFiles.AsNoTracking()
            .Include(x => x.EventOut).Include(x => x.File)
            .Where(e => !e.EventOut.Deleted && e.EventOut.CreatedUtc >= fromDate)
            .OrderBy(e => e.EventOut.CreatedUtc);

        DateTime? lastCreatedUtc = null;

// load trước id để tránh AnyAsync lặp nhiều lần
        var existingExits = new HashSet<Guid>(
            await eventDbContext.Exits.AsNoTracking().Select(e => e.Id).ToListAsync(token));

        var existingEntries = new HashSet<Guid>(
            await eventDbContext.Entries.AsNoTracking().Select(e => e.Id).ToListAsync(token));

        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var existingAccessKeys = new HashSet<Guid>(
            await eventDbContext.AccessKeys.AsNoTracking().Select(ak => ak.Id).ToListAsync(token));

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

            foreach (var eo in exits)
            {
                token.ThrowIfCancellationRequested();

                bool existsExit = existingExits.Contains(eo.Id);
                bool existsCustomer = eo.CustomerId.HasValue && existingCustomers.Contains(eo.CustomerId.Value);

                var eventIn = await parkingDbContext.EventIns
                    .Include(ei => ei.EventInFiles)
                    .ThenInclude(eif => eif.File)
                    .FirstOrDefaultAsync(ei => ei.Id == eo.EventInId, token);

                if (eventIn == null) continue;
                if (!existingAccessKeys.Contains(eventIn.IdentityId)) continue;

                bool existsEntry = existingEntries.Contains(eventIn.Id);

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
                                      existingCustomers.Contains(eventIn.CustomerId.Value))
                            ? eventIn.CustomerId
                            : null
                    };

                    newEntries.Add(entry);
                    await InsertEntryImagesAsync(eventIn, token);

                    existingEntries.Add(entry.Id);
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

                    newExits.Add(exit);
                    await InsertExitImagesAsync(eo, token);

                    existingExits.Add(exit.Id);

                    inserted++;
                    log($"[INSERTED] {eo.Id}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {eo.Id}");
                }
            }

            // 🚀 BulkInsert thay vì Add + SaveChanges
            if (newEntries.Any())
                await eventDbContext.BulkInsertAsync(newEntries, cancellationToken: token);

            if (newExits.Any())
                await eventDbContext.BulkInsertAsync(newExits, cancellationToken: token);

            lastCreatedUtc = exits.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}