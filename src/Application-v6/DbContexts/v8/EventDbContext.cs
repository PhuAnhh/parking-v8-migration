using Microsoft.EntityFrameworkCore;
using parking_v6_to_v8_migration.Configurations.v8;
using parking_v6_to_v8_migration.Entities.v8;

namespace parking_v6_to_v8_migration.DbContexts.v8;

public class EventDbContext : DbContext
{
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<AccessKeyCollection> AccessKeyCollections { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerCollection> CustomerCollections { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccessKeyConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerCollectionConfiguration());
    }
}