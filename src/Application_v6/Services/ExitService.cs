using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class ExitService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public ExitService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
    }

    public async Task InsertExit(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var exits = await _parkingDbContext.EventOuts
            .Where(e => !e.Deleted && e.CreatedUtc >= fromDate)
            .ToListAsync();

        foreach (var eo in exits)
        {
            token.ThrowIfCancellationRequested();

            var existsExit = await _eventDbContext.Exits.AnyAsync(e => e.Id == eo.Id);
            var existsCustomer = await _eventDbContext.Customers.AnyAsync(c => c.Id == eo.CustomerId, token);

            var eventIn = await _parkingDbContext.EventIns.FirstOrDefaultAsync(ei => ei.Id == eo.EventInId, token);
            if (eventIn == null)
                continue;

            var existsEntry = await _eventDbContext.Entries.AnyAsync(e => e.Id == eventIn.Id, token);

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
                                  await _eventDbContext.Customers.AnyAsync(c => c.Id == eventIn.CustomerId, token))
                        ? eventIn.CustomerId
                        : null
                };

                _eventDbContext.Entries.Add(entry);
                log($"[INSERT-ENTRY] Entry {entry.Id} được tạo kèm Exit");
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

                _eventDbContext.Exits.Add(exit);

                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {eo.Id} đã thêm vào Event");
            }
            else
            {
                skipped++;
                log($"[SKIP] {eo.Id} đã tồn tại");
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {exits.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}