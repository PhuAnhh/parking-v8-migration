using Application.DbContexts.v3;
using Application.DbContexts.v8;
using Application.Entities.v3;
using Application.Entities.v8;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class InsertExitsService
{
    private readonly MParkingDbContext _mParkingDbContext;
    private readonly MParkingEventDbContext _mParkingEventDbContext;
    private readonly EventDbContext _eventDbContext;

    public InsertExitsService(MParkingEventDbContext mParkingEventDbContext, EventDbContext eventDbContext,  MParkingDbContext mParkingDbContext)
    {
        _mParkingDbContext = mParkingDbContext;
        _mParkingEventDbContext = mParkingEventDbContext;
        _eventDbContext = eventDbContext;
    }

    public async Task InsertExits(DateTime fromDate, Action<string> log)
    {
        int insertedExits = 0;
        int skippedExits = 0;
        
        var cards = await _mParkingDbContext.Cards
            .Where(c => !c.IsLock && !c.IsDelete)
            .ToListAsync();
        
        var cardGroups = await _mParkingDbContext.CardGroups
            .Select(cg => new
            {
                CardGroupID = cg.CardGroupID.ToString(),
            })
            .ToListAsync();
        
        var exitCardEvents = await _mParkingEventDbContext.ExitCardEventDtos
            .Where(e => e.EventCode == "2" && e.DateTimeOut >= fromDate)
            .ToListAsync();

        var cardEvents = (from c in cards
            join e in exitCardEvents on c.CardNumber equals e.CardNumber
            join cg in cardGroups on c.CardGroupID equals cg.CardGroupID into cgGroup
            from cg in cgGroup.DefaultIfEmpty()
            select new CardEvent
            {
                Id = e.Id,
                CardNumber = e.CardNumber,
                PlateOut = e.PlateOut,
                EventCode = e.EventCode,
                DateTimeOut = e.DateTimeOut,
                PicDirOut = e.PicDirOut,
                LaneIDOut = e.LaneIDOut,
                UserIDOut = e.UserIDOut,
                CustomerName = e.CustomerName,
                Moneys = e.Moneys,
                IsDelete = e.IsDelete,
                ReducedMoney = e.ReducedMoney
            }).ToList();

        foreach (var ce in cardEvents)
        {
            var entry = await _eventDbContext.Entries
                .Include(en => en.AccessKey)
                .Where(en => en.AccessKey.Code == ce.CardNumber && !en.Exited)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                log($"Không tìm thấy sự kiện vào cho thẻ {ce.CardNumber}");
                continue;
            }

            var existedExit = await _eventDbContext.Exits.AnyAsync(x => x.Id == ce.Id);
            if (!existedExit)
            {
                var exit = new Exit
                {
                    Id = ce.Id,
                    EntryId = entry.Id,
                    AccessKeyId = entry.AccessKeyId,
                    PlateNumber = ce.PlateOut,
                    DeviceId = Guid.Parse(ce.LaneIDOut),
                    Amount = (long)ce.Moneys,
                    DiscountAmount = (long)ce.ReducedMoney,
                    Deleted = false,
                    CreatedBy = "admin",
                    CreatedUtc = DateTime.SpecifyKind(ce.DateTimeOut, DateTimeKind.Utc),
                    CustomerId = entry?.CustomerId
                };

                _eventDbContext.Exits.Add(exit);
                entry.Exited = true;

                await _eventDbContext.SaveChangesAsync();

                insertedExits++;
                // log($"Đã thêm exit_id: {exit.Id}");
            }
            else
            {
                skippedExits++;
            }
        }

        log($"---------- Kết quả ----------");
        log($"Tổng {cardEvents.Count} sự kiện");
        log($"Đã di chuyển {insertedExits} sự kiện");
        log($"Tồn tại {skippedExits} sự kiện");
    }
}