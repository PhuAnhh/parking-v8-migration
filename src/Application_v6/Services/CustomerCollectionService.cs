using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8.Event;
using Application_v6.Entities.v8.Resource;

namespace Application_v6.Services;

public class CustomerCollectionService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;

    public CustomerCollectionService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext, ResourceDbContext resourceDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
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
            
            var exitedResource = await _resourceDbContext.CustomerCollections.AnyAsync(cc => cc.Id == cg.Id);
            var exitedEvent = await _eventDbContext.CustomerCollections.AnyAsync(cc => cc.Id == cg.Id);

            if (!exitedResource && !exitedEvent)
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
                
                _resourceDbContext.CustomerCollections.Add(cCResource);
                _eventDbContext.CustomerCollections.Add(cCEvent);
                
                await _resourceDbContext.SaveChangesAsync(token);
                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {cg.Id} - {cg.Name} đã thêm vào Event & Resource" );

            }
            else
            {
                skipped++;
                log($"[SKIP] {cg.Id} - {cg.Name} đã tồn tại" );

            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {customerCollections.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}