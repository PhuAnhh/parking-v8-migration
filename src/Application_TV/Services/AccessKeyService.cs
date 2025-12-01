using Application_TV.DbContexts.v8;
using Application_TV.Entities.v8;
using Application_TV.DbContexts.v6;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.Services;

public class AccessKeyService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertAccessKey(Action<string> log, CancellationToken token)
    {
        int inserted = 0, skipped = 0;
        var batchSize = 5000;

        var existingEventAKeys = new HashSet<Guid>(
            await eventDbContext.AccessKeys.AsNoTracking().Select(x => x.Id).ToListAsync(token));
        var existingResourceAKeys = new HashSet<Guid>(
            await resourceDbContext.AccessKeys.AsNoTracking().Select(x => x.Id).ToListAsync(token));
        var existingCustomers = new HashSet<Guid>(
            await eventDbContext.Customers.AsNoTracking().Select(c => c.Id).ToListAsync(token));

        var identitiesQuery = parkingDbContext.Identites
            .Where(i => !i.Deleted && (i.Type == "Card" || i.Type == "PlateNumber"))
            .OrderBy(i => i.CreatedUtc);

        var vehiclesQuery = parkingDbContext.RegisteredVehicles
            .Where(v => !v.Deleted)
            .OrderBy(v => v.CreatedUtc);

        DateTime? lastIdentityCreated = null;
        DateTime? lastVehicleCreated = null;

        //Identity
        while (true)
        {
            var identities = await identitiesQuery
                .Where(i => lastIdentityCreated == null || i.CreatedUtc > lastIdentityCreated)
                .Take(batchSize)
                .ToListAsync(token);

            if (!identities.Any()) break;

            var newAccessKeys = new List<AccessKey>();

            foreach (var i in identities)
            {
                token.ThrowIfCancellationRequested();

                if (existingEventAKeys.Contains(i.Id) || existingResourceAKeys.Contains(i.Id))
                {
                    skipped++;
                    log($"[SKIPPED - IDENTITY] {i.Id} - {i.Name}");
                    continue;
                }

                AccessKey accessKey;

                if (i.Type == "PlateNumber")
                {
                    
                    accessKey = new AccessKey
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Code = i.Code ?? "",
                        Type = "VEHICLE",
                        CollectionId = i.IdentityGroupId,
                        Status = "IN_USE",
                        Deleted = i.Deleted,
                        CreatedUtc = i.CreatedUtc,
                        UpdatedUtc = i.UpdatedUtc ?? DateTime.UtcNow,
                    };
                }
                else if (i.Type == "Card")
                {
                    accessKey = new AccessKey
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Code = i.Code ?? "",
                        Type = "CARD",
                        CollectionId = i.IdentityGroupId,
                        Status = i.Status switch
                        {
                            "InUse" => "IN_USE",
                            "Locked" => "LOCKED",
                            "NotUse" => "UN_USED"
                        },
                        Deleted = i.Deleted,
                        CreatedUtc = i.CreatedUtc,
                        UpdatedUtc = i.UpdatedUtc ?? DateTime.UtcNow,
                    };
                }
                else
                {
                    continue;
                }

                newAccessKeys.Add(accessKey);
                existingEventAKeys.Add(i.Id);
                existingResourceAKeys.Add(i.Id);

                inserted++;
                log($"[INSERTED - {i.Type.ToUpper()}] {i.Id} - {i.Name}");
            }

            if (newAccessKeys.Any())
            {
                await eventDbContext.BulkInsertAsync(newAccessKeys, cancellationToken: token);
                await resourceDbContext.BulkInsertAsync(newAccessKeys, cancellationToken: token);
            }

            lastIdentityCreated = identities.Last().CreatedUtc;
        }

        //Vehicle
        while (true)
        {
            var vehicles = await vehiclesQuery
                .Where(v => lastVehicleCreated == null || v.CreatedUtc > lastVehicleCreated)
                .Take(batchSize)
                .ToListAsync(token);

            if (!vehicles.Any()) break;

            var newAccessKeys = new List<AccessKey>();

            foreach (var v in vehicles)
            {
                token.ThrowIfCancellationRequested();

                if (existingEventAKeys.Contains(v.Id) || existingResourceAKeys.Contains(v.Id))
                {
                    skipped++;
                    log($"[SKIPPED - VEHICLE] {v.Id} - {v.PlateNumber}");
                    continue;
                }

                var identityGroupId = await (
                        from map in parkingDbContext.RegisteredVehicleIdentityMaps
                        join identity in parkingDbContext.Identites on map.IdentityId equals identity.Id
                        where map.RegisteredVehicleId == v.Id
                              && !identity.Deleted
                        select identity.IdentityGroupId
                    )
                    .FirstOrDefaultAsync(token);

                var ak = new AccessKey
                {
                    Id = v.Id,
                    Name = v.Name,
                    Code = v.PlateNumber,
                    Type = "VEHICLE",
                    CollectionId = identityGroupId,
                    CustomerId = existingCustomers.Contains(v.CustomerId) ? v.CustomerId : null,
                    ExpiredUtc = v.ExpireUtc,
                    Status = "IN_USE",
                    Deleted = v.Deleted,
                    CreatedUtc = v.CreatedUtc,
                    UpdatedUtc = v.UpdatedUtc ?? DateTime.UtcNow
                };

                newAccessKeys.Add(ak);
                existingEventAKeys.Add(v.Id);
                existingResourceAKeys.Add(v.Id);

                inserted++;
                log($"[INSERTED - VEHICLE] {v.Id} - {v.PlateNumber}");
            }

            if (newAccessKeys.Any())
            {
                await eventDbContext.BulkInsertAsync(newAccessKeys, cancellationToken: token);
                await resourceDbContext.BulkInsertAsync(newAccessKeys, cancellationToken: token);
            }

            lastVehicleCreated = vehicles.Last().CreatedUtc;
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");

        var maps = await parkingDbContext.RegisteredVehicleIdentityMaps
            .AsNoTracking()
            .ToListAsync(token);

        var newMetrics = new List<AccessKeyMetric>();

        foreach (var m in maps)
        {
            token.ThrowIfCancellationRequested();

            if (!existingEventAKeys.Contains(m.IdentityId))
            {
                log($"[SKIPPED - METRIC] IdentityId {m.IdentityId} not migrated as AccessKey");
                continue;
            }

            if (!existingEventAKeys.Contains(m.RegisteredVehicleId))
            {
                log($"[SKIPPED - METRIC] Vehicle AccessKeyId {m.RegisteredVehicleId} not found");
                continue;
            }

            newMetrics.Add(new AccessKeyMetric
            {
                AccessKeyId = m.IdentityId,
                RelatedAccessKeyId = m.RegisteredVehicleId,
            });
        }

        if (newMetrics.Any())
        {
            await resourceDbContext.BulkInsertAsync(newMetrics, cancellationToken: token);
            await eventDbContext.BulkInsertAsync(newMetrics, cancellationToken: token);
        }
        else
        {
            log("Không có bản ghi nào cần migrate.");
        }
    }
}