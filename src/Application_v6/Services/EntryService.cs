using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
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
        int inserted = 0;
        int skipped = 0;

        var entries = await _parkingDbContext.EventIns
            .Where(e => !e.Deleted && e.CreatedUtc >= fromDate && e.Status == "Parking")
            .ToListAsync();

        foreach (var ei in entries)
        {
            token.ThrowIfCancellationRequested();
            
            var exitedEntry = await _eventDbContext.Entries.AnyAsync(e => e.Id == ei.Id);
            if (!exitedEntry)
            {
                var entry = new Entry
                {
                    Id = ei.Id,
                    AccessKeyId = ei.IdentityId,
                    DeviceId = ei.LaneId,
                    PlateNumber = ei.PlateNumber,
                    CustomerId = (await _eventDbContext.Customers.AnyAsync(c => c.Id == ei.CustomerId, token))
                        ? ei.CustomerId
                        : null,
                    Exited = ei.Status == "Exited",
                    Amount = ei.TotalPaid,
                    CreatedBy = ei.CreatedBy,
                    Note = ei.Note,
                    Deleted = ei.Deleted,
                    CreatedUtc = ei.CreatedUtc,
                    UpdatedUtc = ei.UpdatedUtc,
                };
                
                _eventDbContext.Entries.Add(entry);
                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {ei.Id} đã thêm vào Event" );
            }
            else
            {
                skipped++;
                log($"[SKIP] {ei.Id} đã tồn tại" );
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {entries.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}