using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

public class CustomerService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertCustomer(Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        const int batchSize = 5000;

        var query = parkingDbContext.Customers
            .AsNoTracking()
            .Where(c => !c.Deleted)
            .OrderBy(c => c.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Customers
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Customers
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(token));

        var existingCollections = new HashSet<Guid>(
            await eventDbContext.CustomerCollections
                .AsNoTracking()
                .Select(x => x.Id)
                .ToListAsync(token));

        while (true)
        {
            var customers = await query
                .Where(c => lastCreatedUtc == null || c.CreatedUtc > lastCreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (!customers.Any()) break;

            var newResourceCustomers = new List<Customer>();
            var newEventCustomers = new List<Customer>();

            foreach (var c in customers)
            {
                token.ThrowIfCancellationRequested();

                if (resourceIds.Contains(c.Id) || eventIds.Contains(c.Id))
                {
                    skipped++;
                    log($"[SKIPPED - EXISTS] {c.Id} - {c.Name}");
                    continue;
                }

                if (c.CustomerGroupId == null || !existingCollections.Contains(c.CustomerGroupId.Value))
                {
                    skipped++;
                    log($"[SKIPPED - NO COLLECTION] {c.Id} - {c.Name}");
                    continue;
                }

                var entity = new Customer
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    CollectionId = c.CustomerGroupId.Value,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Deleted = c.Deleted,
                    CreatedUtc = c.CreatedUtc,
                    UpdatedUtc = c.UpdatedUtc,
                };

                newResourceCustomers.Add(entity);

                newEventCustomers.Add(new Customer
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Code = entity.Code,
                    CollectionId = entity.CollectionId,
                    Address = entity.Address,
                    PhoneNumber = entity.PhoneNumber,
                    Deleted = entity.Deleted,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                });

                resourceIds.Add(c.Id);
                eventIds.Add(c.Id);

                inserted++;
                log($"[INSERTED] {c.Id} - {c.Name}");
            }

            if (newResourceCustomers.Any())
            {
                await resourceDbContext.BulkInsertAsync(
                    newResourceCustomers,
                    cancellationToken: token
                );
            }

            if (newEventCustomers.Any())
            {
                await eventDbContext.BulkInsertAsync(
                    newEventCustomers,
                    cancellationToken: token
                );
            }

            lastCreatedUtc = customers.Last().CreatedUtc;
            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Bỏ qua: {skipped}");
    }
}