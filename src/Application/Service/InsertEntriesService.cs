using Application.Entities.AccessKey;
using Application.Entities.Customer;
using Application.Entities.Device;
using Application.Entities.Entry;
using Microsoft.EntityFrameworkCore;

namespace Application;

public class InsertEntriesService
{
    private readonly CardEventDbContext _cardEventDbContext;
    private readonly EventDbContext _eventDbContext;
    private readonly ResourceDbContext _resourceDbContext;

    public InsertEntriesService(CardEventDbContext cardEventDbContext, EventDbContext eventDbContext,
        ResourceDbContext resourceDbContext)
    {
        _cardEventDbContext = cardEventDbContext;
        _eventDbContext = eventDbContext;
        _resourceDbContext = resourceDbContext;
    }

    public async Task InsertEntries()
    {
        int insertedEntries = 0;

        var cardEvents = await _cardEventDbContext.CardEvents.FromSqlRaw(@"
            SELECT 
                e.Id, 
                e.CardNumber, 
                e.PlateIn, 
                e.EventCode, 
                e.DatetimeIn, 
                e.LaneIDIn, 
                e.UserIDIn, 
                e.CustomerName, 
                CAST(e.Moneys AS BIGINT) AS Moneys, 
                e.IsDelete
            FROM [MPARKINGEVENT_LETO].dbo.tblCardEvent e
            JOIN [MPARKING_LETO].dbo.tblCard c
            ON e.CardNumber = c.CardNumber
            LEFT JOIN [MPARKING].dbo.[tblCardGroup] cg 
            ON CONVERT(varchar(50), cg.CardGroupID) =  CONVERT(varchar(50), c.CardGroupID)
            WHERE c.IsLock = 0 and c.IsDelete = 0 and e.EventCode = 1 and e.DatetimeIn >= '2025-08-20 00:00:00'")
            .ToListAsync();

        foreach (var ce in cardEvents)
        {
            Console.WriteLine($"AC: {ce.CardNumber}");
            var eventAccessKey =
                await _eventDbContext.AccessKeys.FirstOrDefaultAsync(a => a.Code.ToLower() == ce.CardNumber.ToLower());
            if (eventAccessKey == null)
            {
                var resourceAccessKey =
                    await _resourceDbContext.AccessKeys.FirstOrDefaultAsync(a =>
                        a.Code.ToLower() == ce.CardNumber.ToLower());
                if (resourceAccessKey != null)
                {
                    eventAccessKey = new AccessKey
                    {
                        Id = resourceAccessKey.Id,
                        Code = resourceAccessKey.Code,
                        Name = resourceAccessKey.Name,
                        Type = resourceAccessKey.Type,
                        CollectionId = resourceAccessKey.CollectionId,
                        Status = resourceAccessKey.Status,
                    };
                    _eventDbContext.AccessKeys.Add(eventAccessKey);
                    await _eventDbContext.SaveChangesAsync();
                }
            }

            Console.WriteLine($"CN: {ce.CustomerName}");
            var eventCustomer = await _eventDbContext.Customers.FirstOrDefaultAsync(c => c.Name == ce.CustomerName);
            if (eventCustomer == null)
            {
                var resourceCustomer =
                    await _resourceDbContext.Customers.FirstOrDefaultAsync(c => c.Name == ce.CustomerName);
                if (resourceCustomer != null)
                {
                    eventCustomer = new Customer
                    {
                        Id = resourceCustomer.Id,
                        Name = resourceCustomer.Name,
                        Code = resourceCustomer.Code
                    };
                    _eventDbContext.Customers.Add(eventCustomer);
                    await _eventDbContext.SaveChangesAsync();
                }
            }

            Console.WriteLine($"LaneID: {ce.LaneIDIn}");
            var eventDevice = await _eventDbContext.Devices.FirstOrDefaultAsync(d => d.Id == Guid.Parse(ce.LaneIDIn));
            if (eventDevice == null)
            {
                var resourceDevice =
                    await _resourceDbContext.Devices.FirstOrDefaultAsync(d => d.Id == Guid.Parse(ce.LaneIDIn));
                if (resourceDevice != null)
                {
                    eventDevice = new Device
                    {
                        Id = resourceDevice.Id,
                        Name = resourceDevice.Name,
                        Type = resourceDevice.Type,
                    };
                    _eventDbContext.Devices.Add(eventDevice);
                    await _eventDbContext.SaveChangesAsync();
                }
            }

            var existedEntry = await _eventDbContext.Entries.AnyAsync(e => e.Id == ce.Id);
            if (!existedEntry)
            {
                var entry = new Entry
                {
                    Id = ce.Id,
                    PlateNumber = ce.PlateIn,
                    DeviceId = eventDevice.Id,
                    AccessKeyId = eventAccessKey.Id,
                    Exited = false,
                    Amount = ce.Moneys,
                    Deleted = false,
                    CreatedBy = "admin",
                    CreatedUtc = ce.DatetimeIn,
                    CustomerId = eventCustomer?.Id,
                };
                _eventDbContext.Entries.Add(entry);
                await _eventDbContext.SaveChangesAsync();

                insertedEntries++;
                Console.WriteLine($"Đã thêm entry Id = {entry.Id}");
            }
        }
        Console.WriteLine($"Tổng: {insertedEntries} sự kiện");
    }
}