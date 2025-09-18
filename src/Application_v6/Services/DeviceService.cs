using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
namespace Application_v6.Services;

public class DeviceService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public DeviceService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
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
            
            var exitedDevice = await _eventDbContext.Devices.AnyAsync(d => d.Id == l.Id);
            if (!exitedDevice)
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
                
                _eventDbContext.Devices.Add(device);
                await _eventDbContext.SaveChangesAsync();

                inserted++;
            }
            else
            {
                skipped++;
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {lanes.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}