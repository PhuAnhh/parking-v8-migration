using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class AccessKeyCollectionService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public AccessKeyCollectionService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
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
            
            var exitedAccessKeyCollection = await _eventDbContext.AccessKeyCollections.AnyAsync(ac => ac.Id == ig.Id);
            if (!exitedAccessKeyCollection)
            {
                var accessKeyCollection = new AccessKeyCollection
                {
                    Id = ig.Id,
                    Name = ig.Name,
                    Code = ig.Code,
                    VehicleType = ig.VehicleType,
                    Deleted = ig.Deleted,
                    CreatedUtc = ig.CreatedUtc,
                    UpdatedUtc = ig.UpdatedUtc,
                };
                
                _eventDbContext.AccessKeyCollections.Add(accessKeyCollection);
                await _eventDbContext.SaveChangesAsync();

                inserted++;
            }
            else
            {
                skipped++;
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {identityGroups.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}