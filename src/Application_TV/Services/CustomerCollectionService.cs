using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8.Event;
using Application_TV.Entities.v8.Resource;
using Application_TV.DbContexts.v6;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class CustomerCollectionService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext)
{
    public async Task InsertCustomerCollection(Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = parkingDbContext.CustomerGroups.AsNoTracking()
            .Where(cg => !cg.Deleted)
            .OrderBy(cg => cg.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.CustomerCollections.AsNoTracking().Select(cc => cc.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.CustomerCollections.AsNoTracking().Select(cc => cc.Id).ToListAsync(token));

        while (true)
        {
            var cgroups = await query
                .Where(cg => lastCreatedUtc == null || cg.CreatedUtc > lastCreatedUtc)
                .OrderBy(cg => cg.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (cgroups.Count == 0) break;

            var newResourceCollections = new List<ResourceCustomerCollection>();
            var newEventCollections = new List<EventCustomerCollection>();

            foreach (var cg in cgroups)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(cg.Id) || eventIds.Contains(cg.Id);

                if (!exists)
                {
                    var cCResource = new ResourceCustomerCollection
                    {
                        Id = cg.Id,
                        Name = cg.Name,
                        Code = cg.Code,
                        ParentId = cg.ParentId,
                        Deleted = cg.Deleted,
                        CreatedUtc = cg.CreatedUtc,
                        UpdatedUtc = cg.UpdatedUtc,
                    };

                    var cCEvent = new EventCustomerCollection
                    {
                        Id = cg.Id,
                        Name = cg.Name,
                        Code = cg.Code,
                        Deleted = cg.Deleted,
                        CreatedUtc = cg.CreatedUtc,
                        UpdatedUtc = cg.UpdatedUtc,
                    };

                    newResourceCollections.Add(cCResource);
                    newEventCollections.Add(cCEvent);

                    resourceIds.Add(cg.Id);
                    eventIds.Add(cg.Id);

                    inserted++;
                    log($"[INSERTED] {cg.Id} - {cg.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {cg.Id} - {cg.Name}");
                }
            }

            if (newResourceCollections.Any())
                await resourceDbContext.BulkInsertAsync(newResourceCollections, cancellationToken: token);

            if (newEventCollections.Any())
                await eventDbContext.BulkInsertAsync(newEventCollections, cancellationToken: token);

            lastCreatedUtc = cgroups.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}