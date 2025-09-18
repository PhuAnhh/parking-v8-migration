using Microsoft.EntityFrameworkCore;
using parking_v6_to_v8_migration.Configurations.v6;
using Application_v6.Entities.v6;

namespace Application_v6.DbContexts.v6;

public class ParkingDbContext : DbContext
{
    public DbSet<Identity> Identites { get; set; }
    public DbSet<IdentityGroup>  IdentityGroups { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerGroup> CustomerGroups { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<EventIn> EventIns  { get; set; }
    public DbSet<EventOut> EventOuts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IdentityConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityGroupConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new LaneConfiguration());
        modelBuilder.ApplyConfiguration(new EventInConfiguration());
        modelBuilder.ApplyConfiguration(new EventOutConfiguration());
    }
}