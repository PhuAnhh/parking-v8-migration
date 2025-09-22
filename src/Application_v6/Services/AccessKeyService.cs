using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Entities.v8;
using EFCore.BulkExtensions;

namespace Application_v6.Services;

public class AccessKeyService(
    ParkingDbContext parkingDbContext,
    EventDbContext eventDbContext,
    ResourceDbContext resourceDbContext
)
{
    public async Task InsertAccessKey(DateTime fromDate, Action<string> log, CancellationToken token)
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
            .Where(i => !i.Deleted && i.CreatedUtc >= fromDate)
            .OrderBy(i => i.CreatedUtc);

        var vehiclesQuery = parkingDbContext.Vehicles
            .Where(v => !v.Deleted && v.CreatedUtc >= fromDate)
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

                var accessKey = new AccessKey
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
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
                    UpdatedUtc = i.UpdatedUtc,
                };

                newAccessKeys.Add(accessKey);
                existingEventAKeys.Add(i.Id);
                existingResourceAKeys.Add(i.Id);

                inserted++;
                log($"[INSERTED - IDENTITY] {i.Id} - {i.Name}");
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
            var newMetrics = new List<AccessKeyMetric>();

            foreach (var v in vehicles)
            {
                token.ThrowIfCancellationRequested();

                if (existingEventAKeys.Contains(v.Id) || existingResourceAKeys.Contains(v.Id))
                {
                    skipped++;
                    log($"[SKIPPED - VEHICLE] {v.Id} - {v.PlateNumber}");
                    continue;
                }

                var collectionId = await parkingDbContext.VehicleIdentities
                    .Where(vi => vi.VehicleId == v.Id)
                    .Join(parkingDbContext.Identites,
                        vi => vi.IdentityId,
                        i => i.Id,
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
                    CustomerId = existingCustomers.Contains(v.CustomerId) ? v.CustomerId : null,
                    ExpiredUtc = v.ExpireUtc,
                    Status = "IN_USE",
                    Deleted = v.Deleted,
                    CreatedUtc = v.CreatedUtc,
                    UpdatedUtc = v.UpdatedUtc
                };

                newAccessKeys.Add(ak);
                existingEventAKeys.Add(v.Id);
                existingResourceAKeys.Add(v.Id);

                inserted++;
                log($"[INSERTED - VEHICLE] {v.Id} - {v.PlateNumber}");

                var relatedIdentities = await parkingDbContext.VehicleIdentities
                    .Where(vi => vi.VehicleId == v.Id)
                    .ToListAsync(token);

                foreach (var vi in relatedIdentities)
                {
                    var metric = new AccessKeyMetric
                    {
                        AccessKeyId = v.Id,
                        RelatedAccessKeyId = vi.IdentityId
                    };
                    newMetrics.Add(metric);
                }
            }

            if (newAccessKeys.Any())
            {
                await eventDbContext.BulkInsertAsync(newAccessKeys, cancellationToken: token);
                await resourceDbContext.BulkInsertAsync(newAccessKeys, cancellationToken: token);
            }

            if (newMetrics.Any())
            {
                await eventDbContext.BulkInsertAsync(newMetrics, cancellationToken: token);
                await resourceDbContext.BulkInsertAsync(newMetrics, cancellationToken: token);
            }

            lastVehicleCreated = vehicles.Last().CreatedUtc;
        }

        log("========== KẾT QUẢ ==========");
        log($"Thành công: {inserted}");
        log($"Tồn tại: {skipped}");
    }
}