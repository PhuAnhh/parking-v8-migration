using Microsoft.EntityFrameworkCore;
using Application_v6.Configurations.v8;
using Application_v6.Entities.v8;
using Application_v6.Entities.v8.Event;

namespace Application_v6.DbContexts.v8;

public class EventDbContext : DbContext
{
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<AccessKeyCollection> AccessKeyCollections { get; set; }
    public DbSet<AccessKeyMetric> AccessKeyMetrics { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<EventCustomerCollection> CustomerCollections { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<EntryImage> EntryImages { get; set; }
    public DbSet<Exit> Exits { get; set; }
    public DbSet<ExitImage> ExitImages { get; set; }
    
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccessKeyConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyMetricConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new Application_v6.Configurations.v8.Event.CustomerCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new EntryConfiguration());
        modelBuilder.ApplyConfiguration(new EntryImageConfiguration());
        modelBuilder.ApplyConfiguration(new ExitConfiguration());
        modelBuilder.ApplyConfiguration(new ExitImageConfiguration());
    }
}