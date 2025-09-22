using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

public class DeviceService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertLane(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = parkingDbContext.Lanes.AsNoTracking()
            .Where(c => !c.Deleted && c.CreatedUtc >= fromDate)
            .OrderBy(c => c.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var lanes = await query
                .Where(c => lastCreatedUtc == null || c.CreatedUtc > lastCreatedUtc)
                .OrderBy(c => c.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (lanes.Count == 0) break;

            var newResourceDevices = new List<Device>();
            var newEventDevices = new List<Device>();

            foreach (var l in lanes)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(l.Id) || eventIds.Contains(l.Id);

                if (!exists)
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

                    newResourceDevices.Add(device);
                    newEventDevices.Add(new Device
                    {
                        Id = device.Id,
                        Name = device.Name,
                        Code = device.Code,
                        Type = device.Type,
                        Enabled = device.Enabled,
                        Deleted = device.Deleted,
                        CreatedUtc = device.CreatedUtc,
                        UpdatedUtc = device.UpdatedUtc,
                    });

                    resourceIds.Add(device.Id);
                    eventIds.Add(device.Id);

                    inserted++;
                    log($"[INSERTED] {l.Id} - {l.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {l.Id} - {l.Name}");
                }
            }

            if (newResourceDevices.Any())
                await resourceDbContext.BulkInsertAsync(newResourceDevices, cancellationToken: token);

            if (newEventDevices.Any())
                await eventDbContext.BulkInsertAsync(newEventDevices, cancellationToken: token);

            lastCreatedUtc = lanes.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}