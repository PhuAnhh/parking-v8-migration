using Application.Entities.v3;
using Microsoft.EntityFrameworkCore;
using Application.Entities.v3.Models;

namespace Application.DbContexts.v3;

public class MParkingEventDbContext : DbContext
{
    public DbSet<EntryCardEventDto> EntryCardEventDtos { get; set; }
    public DbSet<ExitCardEventDto> ExitCardEventDtos { get; set; }
    public DbSet<CardEvent>  CardEvents { get; set; }
    
    public MParkingEventDbContext(DbContextOptions<MParkingEventDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntryCardEventDto>(entity =>
        {
            entity.ToTable("tblCardEvent", "dbo");
            entity.Property(ce => ce.Id).HasColumnName("Id");
            entity.Property(ce => ce.EventCode).HasColumnName("EventCode");
            entity.Property(ce => ce.CardNumber).HasColumnName("CardNumber");
            entity.Property(ce => ce.DatetimeIn).HasColumnName("DatetimeIn");
            entity.Property(ce => ce.LaneIDIn).HasColumnName("LaneIDIn");
            entity.Property(ce => ce.UserIDIn).HasColumnName("UserIDIn");
            entity.Property(ce => ce.PlateIn).HasColumnName("PlateIn");
            entity.Property(ce => ce.Moneys).HasColumnName("Moneys")
                .HasColumnType("numeric(18, 0)")
                .HasConversion<long>();
            entity.Property(ce => ce.CustomerName).HasColumnName("CustomerName");
            entity.Property(ce => ce.IsDelete).HasColumnName("IsDelete");
        });

        modelBuilder.Entity<ExitCardEventDto>(entity =>
        {
            entity.ToView("tblCardEvent", "dbo");
            entity.Property(ce => ce.Id).HasColumnName("Id");
            entity.Property(ce => ce.EventCode).HasColumnName("EventCode");
            entity.Property(ce => ce.CardNumber).HasColumnName("CardNumber");
            entity.Property(ce => ce.DateTimeOut).HasColumnName("DatetimeOut");
            entity.Property(ce => ce.LaneIDOut).HasColumnName("LaneIDOut");
            entity.Property(ce => ce.UserIDOut).HasColumnName("UserIDOut");
            entity.Property(ce => ce.PlateOut).HasColumnName("PlateOut");
            entity.Property(ce => ce.Moneys).HasColumnName("Moneys")
                .HasColumnType("numeric(18, 0)")
                .HasConversion<long>();
            entity.Property(ce => ce.CustomerName).HasColumnName("CustomerName");
            entity.Property(ce => ce.IsDelete).HasColumnName("IsDelete");
            entity.Property(ce => ce.ReducedMoney).HasColumnName("ReducedMoney");
        });
    }
}
