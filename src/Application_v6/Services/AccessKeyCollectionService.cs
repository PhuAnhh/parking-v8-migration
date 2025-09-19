using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class AccessKeyCollectionService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;

    public AccessKeyCollectionService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext, ResourceDbContext resourceDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
    }

    public async Task InsertAccessKeyCollection(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var identityGroups = await _parkingDbContext.IdentityGroups
            .Where(ig => !ig.Deleted && ig.CreatedUtc >= fromDate)
            .ToListAsync();

        foreach (var ig in identityGroups)
        {
            token.ThrowIfCancellationRequested();
            
            var existsResource = await _eventDbContext.AccessKeyCollections.AnyAsync(ac => ac.Id == ig.Id);
            var existsEvent = await _resourceDbContext.AccessKeyCollections.AnyAsync(ac => ac.Id == ig.Id);
                
            if (!existsResource && !existsEvent)
            {
                var aKCResource = new AccessKeyCollection
                {
                    Id = ig.Id,
                    Name = ig.Name,
                    Code = ig.Code,
                    VehicleType = ig.VehicleType.ToUpper(),
                    Deleted = ig.Deleted,
                    CreatedUtc = ig.CreatedUtc,
                    UpdatedUtc = ig.UpdatedUtc,
                };
                
                var aKCEvent = new AccessKeyCollection
                {
                    Id = ig.Id,
                    Name = ig.Name,
                    Code = ig.Code,
                    VehicleType = ig.VehicleType.ToUpper(),
                    Deleted = ig.Deleted,
                    CreatedUtc = ig.CreatedUtc,
                    UpdatedUtc = ig.UpdatedUtc,
                };
                
                _resourceDbContext.AccessKeyCollections.Add(aKCResource);
                _eventDbContext.AccessKeyCollections.Add(aKCEvent);
                
                await _eventDbContext.SaveChangesAsync(token);
                await _resourceDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {ig.Id} - {ig.Name} đã thêm vào Event & Resource");
            }
            else
            {
                skipped++;
                log($"[SKIP] {ig.Id} - {ig.Name} đã tồn tại");
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {identityGroups.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}