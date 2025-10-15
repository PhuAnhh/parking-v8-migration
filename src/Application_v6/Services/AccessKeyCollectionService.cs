using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

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
                        VehicleType = ig.VehicleType?.ToUpper(),
                        Deleted = ig.Deleted,
                        CreatedUtc = ig.CreatedUtc,
                        UpdatedUtc = ig.UpdatedUtc,
                    };

                    var aKCEvent = new AccessKeyCollection
                    {
                        Id = ig.Id,
                        Name = ig.Name,
                        Code = ig.Code,
                        VehicleType = ig.VehicleType?.ToUpper(),
                        Deleted = ig.Deleted,
                        CreatedUtc = ig.CreatedUtc,
                        UpdatedUtc = ig.UpdatedUtc,
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
}