using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class DeviceService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;

    public DeviceService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext, ResourceDbContext resourceDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
    }
    
    public async Task InsertDevice(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var lanes = await _parkingDbContext.Lanes
            .Where(c => !c.Deleted && c.CreatedUtc >= fromDate)
            .ToListAsync();

        foreach (var l in lanes)
        {
            token.ThrowIfCancellationRequested();
            
            var existsResource = await _resourceDbContext.Devices.AnyAsync(d => d.Id == l.Id);
            var existsEvent = await _eventDbContext.Devices.AnyAsync(d => d.Id == l.Id);
            
            if (!existsResource && !existsEvent)
            {
                var device = new Device
                {
                    Id = l.Id,
                    Name = l.Name,
                    Code = l.Code,
                    Type = "LANE",
                    Enabled = l.Enabled,
                    Deleted = l.Deleted,
                    CreatedUtc = l.CreatedUtc,
                    UpdatedUtc = l.UpdatedUtc,
                };
                
                _resourceDbContext.Devices.Add(device);
                _eventDbContext.Devices.Add(device);
                
                await _resourceDbContext.SaveChangesAsync(token);
                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {l.Id} - {l.Name} đã thêm vào Event & Resource" );
            }
            else
            {
                skipped++;
                log($"[SKIP] {l.Id} - {l.Name} đã tồn tại" );
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {lanes.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}