using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

public class DeviceService(
    DeviceDbContext deviceDbContext,
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertGate(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = deviceDbContext.Gates.AsNoTracking()
            .Where(g => !g.Deleted && g.CreatedUtc >= fromDate)
            .OrderBy(g => g.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var gates = await query
                .Where(g => lastCreatedUtc == null || g.CreatedUtc > lastCreatedUtc)
                .OrderBy(g => g.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (gates.Count == 0) break;

            var newResourceDevices = new List<Device>();
            var newEventDevices = new List<Device>();

            foreach (var g in gates)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(g.Id) || eventIds.Contains(g.Id);

                if (!exists)
                {
                    var device = new Device
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Code = g.Code,
                        Type = "GATE",
                        Enabled = g.Enabled,
                        Deleted = g.Deleted,
                        CreatedUtc = g.CreatedUtc,
                        UpdatedUtc = g.UpdatedUtc,
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
                    log($"[INSERTED] {g.Id} - {g.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {g.Id} - {g.Name}");
                }
            }

            if (newResourceDevices.Any())
                await resourceDbContext.BulkInsertAsync(newResourceDevices, cancellationToken: token);

            if (newEventDevices.Any())
                await eventDbContext.BulkInsertAsync(newEventDevices, cancellationToken: token);

            lastCreatedUtc = gates.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
    
    public async Task InsertComputer(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = deviceDbContext.Computers.AsNoTracking()
            .Where(c => !c.Deleted && c.CreatedUtc >= fromDate)
            .OrderBy(c => c.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var computers = await query
                .Where(c => lastCreatedUtc == null || c.CreatedUtc > lastCreatedUtc)
                .OrderBy(c => c.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (computers.Count == 0) break;

            var newResourceDevices = new List<Device>();
            var newEventDevices = new List<Device>();

            foreach (var c in computers)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(c.Id) || eventIds.Contains(c.Id);

                if (!exists)
                {
                    var device = new Device
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = "COMPUTER",
                        Enabled = c.Enabled,
                        Deleted = c.Deleted,
                        CreatedUtc = c.CreatedUtc,
                        UpdatedUtc = c.UpdatedUtc,
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
                    log($"[INSERTED] {c.Id} - {c.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {c.Id} - {c.Name}");
                }
            }

            if (newResourceDevices.Any())
                await resourceDbContext.BulkInsertAsync(newResourceDevices, cancellationToken: token);

            if (newEventDevices.Any())
                await eventDbContext.BulkInsertAsync(newEventDevices, cancellationToken: token);

            lastCreatedUtc = computers.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
    
    public async Task InsertCamera(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = deviceDbContext.Cameras.AsNoTracking()
            .Where(c => !c.Deleted && c.CreatedUtc >= fromDate)
            .OrderBy(c => c.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var cameras = await query
                .Where(c => lastCreatedUtc == null || c.CreatedUtc > lastCreatedUtc)
                .OrderBy(c => c.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (cameras.Count == 0) break;

            var newResourceDevices = new List<Device>();
            var newEventDevices = new List<Device>();

            foreach (var c in cameras)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(c.Id) || eventIds.Contains(c.Id);

                if (!exists)
                {
                    var device = new Device
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.Code,
                        Type = "CAMERA",
                        Enabled = c.Enabled,
                        Deleted = c.Deleted,
                        CreatedUtc = c.CreatedUtc,
                        UpdatedUtc = c.UpdatedUtc,
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
                    log($"[INSERTED] {c.Id} - {c.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {c.Id} - {c.Name}");
                }
            }

            if (newResourceDevices.Any())
                await resourceDbContext.BulkInsertAsync(newResourceDevices, cancellationToken: token);

            if (newEventDevices.Any())
                await eventDbContext.BulkInsertAsync(newEventDevices, cancellationToken: token);

            lastCreatedUtc = cameras.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
    
    public async Task InsertControlUnit(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = deviceDbContext.ControlUnits.AsNoTracking()
            .Where(cu => !cu.Deleted && cu.CreatedUtc >= fromDate)
            .OrderBy(cu => cu.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var controlUnits = await query
                .Where(cu => lastCreatedUtc == null || cu.CreatedUtc > lastCreatedUtc)
                .OrderBy(cu => cu.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (controlUnits.Count == 0) break;

            var newResourceDevices = new List<Device>();
            var newEventDevices = new List<Device>();

            foreach (var cu in controlUnits)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(cu.Id) || eventIds.Contains(cu.Id);

                if (!exists)
                {
                    var device = new Device
                    {
                        Id = cu.Id,
                        Name = cu.Name,
                        Code = cu.Code,
                        Type = "CONTROL_UNIT",
                        Enabled = cu.Enabled,
                        Deleted = cu.Deleted,
                        CreatedUtc = cu.CreatedUtc,
                        UpdatedUtc = cu.UpdatedUtc,
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
                    log($"[INSERTED] {cu.Id} - {cu.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {cu.Id} - {cu.Name}");
                }
            }

            if (newResourceDevices.Any())
                await resourceDbContext.BulkInsertAsync(newResourceDevices, cancellationToken: token);

            if (newEventDevices.Any())
                await eventDbContext.BulkInsertAsync(newEventDevices, cancellationToken: token);

            lastCreatedUtc = controlUnits.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
    
    public async Task InsertLane(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = parkingDbContext.Lanes.AsNoTracking()
            .Where(l => !l.Deleted && l.CreatedUtc >= fromDate)
            .OrderBy(l => l.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var lanes = await query
                .Where(l => lastCreatedUtc == null || l.CreatedUtc > lastCreatedUtc)
                .OrderBy(l => l.CreatedUtc)
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
    
    public async Task InsertLed(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = deviceDbContext.Leds.AsNoTracking()
            .Where(l => !l.Deleted && l.CreatedUtc >= fromDate)
            .OrderBy(l => l.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Devices.AsNoTracking().Select(d => d.Id).ToListAsync(token));

        while (true)
        {
            var leds = await query
                .Where(l => lastCreatedUtc == null || l.CreatedUtc > lastCreatedUtc)
                .OrderBy(l => l.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (leds.Count == 0) break;

            var newResourceDevices = new List<Device>();
            var newEventDevices = new List<Device>();

            foreach (var l in leds)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(l.Id) || eventIds.Contains(l.Id);

                if (!exists)
                {
                    var device = new Device
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Type = "LED",
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

            lastCreatedUtc = leds.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}