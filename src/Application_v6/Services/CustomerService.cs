using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class CustomerService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public CustomerService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
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
            
            var exitedCustomer = await _eventDbContext.Customers.AnyAsync(c => c.Id == c.Id);
            if (!exitedCustomer)
            {
                var customer = new Customer
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    CollectionId = c.CustomerGroupId,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    Deleted = c.Deleted,
                    CreatedUtc = c.CreatedUtc,
                    UpdatedUtc = c.UpdatedUtc,
                };
                
                _eventDbContext.Customers.Add(customer);
                await _eventDbContext.SaveChangesAsync();

                inserted++;
            }
            else
            {
                skipped++;
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {customers.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}