using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class CustomerService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;

    public CustomerService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext,
        ResourceDbContext resourceDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
    }

    public async Task InsertCustomer(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var customers = await _parkingDbContext.Customers
            .Where(c => !c.Deleted && c.CreatedUtc >= fromDate)
            .ToListAsync();

        foreach (var c in customers)
        {
            token.ThrowIfCancellationRequested();

            var exitedResource = await _resourceDbContext.Customers.AnyAsync(c8 => c8.Id == c.Id);
            var exitedEvent = await _eventDbContext.Customers.AnyAsync(c8 => c8.Id == c.Id);

            if (!exitedResource && !exitedEvent)
            {
                var cResource = new Customer
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    CollectionId = c.CustomerGroupId != null 
                                   && await _resourceDbContext.CustomerCollections.AnyAsync(col => col.Id == c.CustomerGroupId, token)
                        ? c.CustomerGroupId 
                        : null,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Deleted = c.Deleted,
                    CreatedUtc = c.CreatedUtc,
                    UpdatedUtc = c.UpdatedUtc,
                };

                var cEvent = new Customer
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    CollectionId = c.CustomerGroupId != null 
                                   && await _eventDbContext.CustomerCollections.AnyAsync(col => col.Id == c.CustomerGroupId, token)
                        ? c.CustomerGroupId 
                        : null,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Deleted = c.Deleted,
                    CreatedUtc = c.CreatedUtc,
                    UpdatedUtc = c.UpdatedUtc,
                };

                _resourceDbContext.Customers.Add(cResource);
                _eventDbContext.Customers.Add(cEvent);

                await _resourceDbContext.SaveChangesAsync(token);
                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {c.Id} - {c.Name} đã thêm vào Event & Resource");
            }
            else
            {
                skipped++;
                log($"[SKIP] {c.Id} - {c.Name} đã tồn tại");
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {customers.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}