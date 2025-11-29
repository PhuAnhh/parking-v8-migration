using Microsoft.EntityFrameworkCore;
using Application_TV.Configurations.v8;
using Application_TV.Entities.v8;
using Application_TV.Entities.v8.Resource;

namespace Application_TV.DbContexts.v8;

public class ResourceDbContext : DbContext
{
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<AccessKeyCollection> AccessKeyCollections { get; set; }
    public DbSet<AccessKeyMetric> AccessKeyMetrics { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ResourceCustomerCollection> CustomerCollections { get; set; }
    public DbSet<Device> Devices { get; set; }

    public ResourceDbContext(DbContextOptions<ResourceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccessKeyConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new AccessKeyMetricConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(
            new Application_TV.Configurations.v8.Resource.CustomerCollectionConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
    }
}