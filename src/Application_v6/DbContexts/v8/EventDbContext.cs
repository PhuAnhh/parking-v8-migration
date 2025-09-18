using Microsoft.EntityFrameworkCore;
using parking_v6_to_v8_migration.Configurations.v8;
using Application_v6.Entities.v8;

namespace Application_v6.DbContexts.v8;

public class EventDbContext : DbContext
{
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<AccessKeyCollection> AccessKeyCollections { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerCollection> CustomerCollections { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<Exit> Exits { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccessKeyConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new EntryConfiguration());
        modelBuilder.ApplyConfiguration(new ExitConfiguration());
        
    }
}