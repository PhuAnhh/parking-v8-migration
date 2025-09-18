using Microsoft.EntityFrameworkCore;
using parking_v6_to_v8_migration.Configurations.v6;
using parking_v6_to_v8_migration.Entities.v6;

namespace parking_v6_to_v8_migration.DbContexts.v6;

public class ParkingDbContext : DbContext
{
    public DbSet<Identity> Identites { get; set; }
    public DbSet<IdentityGroup>  IdentityGroups { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerGroup> CustomerGroups { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IdentityConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityGroupConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
    }
}