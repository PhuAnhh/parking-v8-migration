using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;

namespace Application_v6.Services;

public class AccessKeyService
{
    private readonly ParkingDbContext _parkingDbContext;
    private readonly EventDbContext _eventDbContext;

    public AccessKeyService(ParkingDbContext parkingDbContext, EventDbContext eventDbContext)
    {
        _parkingDbContext = parkingDbContext;
        _eventDbContext = eventDbContext;
    }
    
    public async Task InsertAccessKey(DateTime fromDate, Action<string> log, CancellationToken token)
    {
        int inserted = 0;
        int skipped = 0;

        var identites = await _parkingDbContext.Identites
            .Where(i => !i.Deleted && i.CreatedUtc >= fromDate)
            .ToListAsync();

        foreach (var i in identites)
        {
            token.ThrowIfCancellationRequested();
            
            var exitedIdentity = await _eventDbContext.AccessKeys.AnyAsync(ak => ak.Id == i.Id);
            if (!exitedIdentity)
            {
                var accessKey = new AccessKey
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Type = i.Type,
                    CollectionId = i.IdentityGroupId,
                    Status = i.Status,
                    Deleted = i.Deleted,
                    CreatedUtc = i.CreatedUtc,
                    UpdatedUtc = i.UpdatedUtc,
                };
                
                _eventDbContext.AccessKeys.Add(accessKey);
                await _eventDbContext.SaveChangesAsync();

                inserted++;
            }
            else
            {
                skipped++;
            }
        }

        log("========== KẾT QUẢ ==========");
        log($"Tổng: {identites.Count}");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}