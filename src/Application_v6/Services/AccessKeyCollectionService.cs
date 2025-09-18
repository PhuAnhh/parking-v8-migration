using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;

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
}