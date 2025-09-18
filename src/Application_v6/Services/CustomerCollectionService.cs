using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class CustomerCollectionService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public CustomerCollectionService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
    }

    public async Task InsertCustomerCollection(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var customerCollections = await _parkingDbContext.CustomerGroups
            .Where(cg => !cg.Deleted && cg.CreatedUtc >= fromDate)
            .ToListAsync();

        foreach (var cg in customerCollections)
        {
            token.ThrowIfCancellationRequested();
            
            var exitedCustomerCollection = await _eventDbContext.CustomerCollections.AnyAsync(cc => cc.Id == cg.Id);
            if (!exitedCustomerCollection)
            {
                var customerCollection = new CustomerCollection
                {
                    Id = cg.Id,
                    Name = cg.Name,
                    Code = cg.Code,
                    Deleted = cg.Deleted,
                    ParentId = cg.ParentId,
                    CreatedUtc = cg.CreatedUtc,
                    UpdatedUtc = cg.UpdatedUtc,
                };
                
                _eventDbContext.CustomerCollections.Add(customerCollection);
                await _eventDbContext.SaveChangesAsync();

                inserted++;
            }
            else
            {
                skipped++;
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {customerCollections.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}