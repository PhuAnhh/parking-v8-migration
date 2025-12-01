using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8;
using Application_TV.DbContexts.v6;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class AccessKeyCollectionService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertAccessKeyCollection(Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = parkingDbContext.IdentityGroups.AsNoTracking()
            .Where(ig => !ig.Deleted)
            .OrderBy(ig => ig.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.AccessKeyCollections.AsNoTracking().Select(ac => ac.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.AccessKeyCollections.AsNoTracking().Select(ac => ac.Id).ToListAsync(token));

        while (true)
        {
            var igroups = await query
                .Where(ig => lastCreatedUtc == null || ig.CreatedUtc > lastCreatedUtc)
                .OrderBy(ig => ig.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (igroups.Count == 0) break;

            var newResourceCollections = new List<AccessKeyCollection>();
            var newEventCollections = new List<AccessKeyCollection>();

            foreach (var ig in igroups)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(ig.Id) || eventIds.Contains(ig.Id);

                if (!exists)
                {
                    var aKCResource = new AccessKeyCollection
                    {
                        Id = ig.Id,
                        Name = ig.Name,
                        Code = ig.Code,
                        VehicleType = MapVehicleType(ig.VehicleTypeId),
                        Enabled = ig.Enabled,
                        Deleted = ig.Deleted,
                        CreatedUtc = ig.CreatedUtc,
                        UpdatedUtc = ig.UpdatedUtc ?? DateTime.UtcNow,
                    };

                    var aKCEvent = new AccessKeyCollection
                    {
                        Id = ig.Id,
                        Name = ig.Name,
                        Code = ig.Code,
                        VehicleType = MapVehicleType(ig.VehicleTypeId),
                        Enabled = ig.Enabled,
                        Deleted = ig.Deleted,
                        CreatedUtc = ig.CreatedUtc,
                        UpdatedUtc = ig.UpdatedUtc ?? DateTime.UtcNow,
                    };

                    newResourceCollections.Add(aKCResource);
                    newEventCollections.Add(aKCEvent);

                    resourceIds.Add(ig.Id);
                    eventIds.Add(ig.Id);

                    inserted++;
                    log($"[INSERTED] {ig.Id} - {ig.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {ig.Id} - {ig.Name}");
                }
            }

            if (newResourceCollections.Any())
                await resourceDbContext.BulkInsertAsync(newResourceCollections, cancellationToken: token);

            if (newEventCollections.Any())
                await eventDbContext.BulkInsertAsync(newEventCollections, cancellationToken: token);

            lastCreatedUtc = igroups.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }

    private static string MapVehicleType(int id) =>
        id switch
        {
            1 or 2 => "CAR",
            3 or 4 => "MOTORBIKE",
            5 or 6 => "BIKE",
            _ => throw new ArgumentOutOfRangeException()
        };

}