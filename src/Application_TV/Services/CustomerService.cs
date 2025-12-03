using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8;
using Application_TV.DbContexts.v6;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class CustomerService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertCustomer(Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        int batchSize = 5000;

        var query = parkingDbContext.Customers.AsNoTracking()
            .Where(c => !c.Deleted)
            .OrderBy(c => c.CreatedUtc);

        DateTime? lastCreatedUtc = null;

        var resourceIds = new HashSet<Guid>(
            await resourceDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var eventIds = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        while (true)
        {
            var customers = await query
                .Where(c => lastCreatedUtc == null || c.CreatedUtc > lastCreatedUtc)
                .OrderBy(c => c.CreatedUtc)
                .Take(batchSize)
                .ToListAsync(token);

            if (customers.Count == 0) break;

            var newResourceCustomers = new List<Customer>();
            var newEventCustomers = new List<Customer>();

            foreach (var c in customers)
            {
                token.ThrowIfCancellationRequested();

                bool exists = resourceIds.Contains(c.Id) || eventIds.Contains(c.Id);

                if (!exists)
                {
                    var entity = new Customer
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.Code,
                        CollectionId = c.CustomerGroupId,
                        Address = c.Address,
                        PhoneNumber = c.PhoneNumber,
                        Deleted = c.Deleted,
                        CreatedUtc = DateTime.UtcNow,
                        UpdatedUtc = DateTime.UtcNow, 
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
                        CreatedUtc = DateTime.UtcNow,
                        UpdatedUtc = DateTime.UtcNow,
                    });

                    resourceIds.Add(c.Id);
                    eventIds.Add(c.Id);

                    inserted++;
                    log($"[INSERTED] {c.Id} - {c.Name}");
                }
                else
                {
                    skipped++;
                    log($"[SKIPPED] {c.Id} - {c.Name}");
                }
            }

            if (newResourceCustomers.Any())
                await resourceDbContext.BulkInsertAsync(newResourceCustomers, cancellationToken: token);

            if (newEventCustomers.Any())
                await eventDbContext.BulkInsertAsync(newEventCustomers, cancellationToken: token);

            lastCreatedUtc = customers.Last().CreatedUtc;

            log($"Đã xử lý tổng cộng: {inserted + skipped}");
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}