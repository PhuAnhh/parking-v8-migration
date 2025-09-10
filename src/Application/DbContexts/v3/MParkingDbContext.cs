using Application.Entities.v3;
using Application.Entities.v8;
using Microsoft.EntityFrameworkCore;

namespace Application.DbContexts.v3;

public class MParkingDbContext : DbContext
{
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardGroup> CardGroups { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    
    public MParkingDbContext(DbContextOptions<MParkingDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("tblCard", "dbo");
            entity.HasKey(c => c.CardId);
            entity.Property(c => c.CardId).HasColumnName("CardId");
            entity.Property(c => c.CardNumber).HasColumnName("CardNumber");
            entity.Property(c => c.CustomerID).HasColumnName("CustomerID");
            entity.Property(c => c.CardGroupID).HasColumnName("CardGroupID");
            entity.Property(c => c.IsLock).HasColumnName("IsLock");
            entity.Property(c => c.IsDelete).HasColumnName("IsDelete");
        });

        modelBuilder.Entity<CardGroup>(entity =>
        {
            entity.ToTable("tblCardGroup", "dbo");
            entity.Property(cg => cg.CardGroupID).HasColumnName("CardGroupID");
        });

        modelBuilder.Entity<Lane>(entity =>
        {
            entity.ToTable("tblLane", "dbo");
            entity.HasKey(l => l.LaneID);
            entity.Property(l => l.LaneID).HasColumnName("LaneID");
            entity.Property(l => l.LaneName).HasColumnName("LaneName");
        });
    }
}