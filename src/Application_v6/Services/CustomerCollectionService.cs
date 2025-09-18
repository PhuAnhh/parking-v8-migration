using Microsoft.EntityFrameworkCore;
using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;

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
}