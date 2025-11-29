using Application_TV.Configurations.v6;
using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;

namespace Application_v6.DbContexts.v6;

public class ParkingDbContext : DbContext
{
    public DbSet<Identity> Identites { get; set; }
    public DbSet<IdentityGroup> IdentityGroups { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerGroup> CustomerGroups { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<EventIn> EventIns { get; set; }
    public DbSet<EventOut> EventOuts { get; set; }

    public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IdentityConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityGroupConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new LaneConfiguration());
        modelBuilder.ApplyConfiguration(new EventInConfiguration());
        modelBuilder.ApplyConfiguration(new EventOutConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
        // Add your database provider configuration, e.g.,
        // optionsBuilder.UseSqlServer("YourConnectionString");
    }
}