using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class AccessKeyService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;

    public AccessKeyService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext, ResourceDbContext resourceDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
    }
    
    public async Task InsertAccessKey(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var identites = await _parkingDbContext.Identites
            .Where(i => !i.Deleted && i.CreatedUtc >= fromDate)
            .ToListAsync(token);
        
        var vehicles = await _parkingDbContext.Vehicles
            .Where(v => !v.Deleted && v.CreatedUtc >= fromDate)
            .ToListAsync(token);

        foreach (var i in identites)
        {
            token.ThrowIfCancellationRequested();
            
            var exitedResource = await _resourceDbContext.AccessKeys.AnyAsync(ak => ak.Id == i.Id);
            var exitedEvent = await _eventDbContext.AccessKeys.AnyAsync(ak => ak.Id == i.Id);

            if (!exitedResource && !exitedEvent)
            {
                var aKResource = new AccessKey
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Type = "CARD",
                    CollectionId = i.IdentityGroupId,
                    Status = i.Status switch
                    {
                        "InUse"  => "IN_USE",
                        "Locked" => "LOCKED",
                        "NotUse" => "UN_USED"
                    },
                    Deleted = i.Deleted,
                    CreatedUtc = i.CreatedUtc,
                    UpdatedUtc = i.UpdatedUtc,
                };
                
                var aKEvent = new AccessKey
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Type = i.Type.ToUpper(),
                    CollectionId = i.IdentityGroupId,
                    Status = i.Status switch
                    {
                        "InUse"  => "IN_USE",
                        "Locked" => "LOCKED",
                        "NotUse" => "UN_USED"
                    },
                    Deleted = i.Deleted,
                    CreatedUtc = i.CreatedUtc,
                    UpdatedUtc = i.UpdatedUtc,
                };
                
                _resourceDbContext.AccessKeys.Add(aKResource);
                _eventDbContext.AccessKeys.Add(aKEvent);
                
                await _resourceDbContext.SaveChangesAsync(token);
                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {i.Id} - {i.Name} đã thêm vào Event & Resource" );
            }
            else
            {
                skipped++;
                log($"[SKIP] {i.Id} - {i.Name} đã tồn tại");
            }
        }

        foreach (var v in vehicles)
        {
            token.ThrowIfCancellationRequested();

            var existsResource = await _resourceDbContext.AccessKeys.AnyAsync(ak => ak.Id == v.Id, token);
            var existsEvent    = await _eventDbContext.AccessKeys.AnyAsync(ak => ak.Id == v.Id, token);
            
            var existsCustomer = await _eventDbContext.Customers.AnyAsync(c => c.Id == v.CustomerId, token);
            
            if (!existsResource && !existsEvent)
            {
                var collectionId = await _parkingDbContext.VehicleIdentities
                    .Where(vi => vi.VehicleId == v.Id)
                    .Join(_parkingDbContext.Identites,
                        vi => vi.IdentityId,
                        i  => i.Id,
                        (vi, i) => i.IdentityGroupId)
                    .Distinct()
                    .FirstOrDefaultAsync(token);
                    
                var ak = new AccessKey
                {
                    Id = v.Id,
                    Name = v.Name, 
                    Code = v.PlateNumber,
                    Type = "VEHICLE",
                    CollectionId = collectionId,
                    CustomerId = existsCustomer ? v.CustomerId : null,
                    ExpiredUtc = v.ExpireUtc ?? DateTime.UtcNow,
                    Status = "IN_USE",
                    Deleted = v.Deleted,
                    CreatedUtc = v.CreatedUtc,
                    UpdatedUtc = v.UpdatedUtc
                };

                _resourceDbContext.AccessKeys.Add(ak);
                _eventDbContext.AccessKeys.Add(ak);
                await _resourceDbContext.SaveChangesAsync(token);
                await _eventDbContext.SaveChangesAsync(token);

                inserted++;
                log($"[INSERT] {v.Id} - {v.PlateNumber} (Vehicle) đã thêm vào Event & Resource");
            }
            else
            {
                skipped++;
                log($"[SKIP] {v.Id} - {v.PlateNumber} (Vehicle) đã tồn tại");
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: [Định danh] - {identites.Count}, [Phương tiện] - {vehicles.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}