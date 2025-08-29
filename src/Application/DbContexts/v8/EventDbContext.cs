using Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.DbContexts.v8;

public class EventDbContext : DbContext
{
    public DbSet<Entry> Entries { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<Customer> Customers { get; set; }
    
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entry>(entity =>
        {
            entity.ToTable("entries");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PlateNumber).HasColumnName("plate_number");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.AccessKeyId).HasColumnName("access_key_id");
            entity.Property(e => e.Exited).HasColumnName("exited");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
        });
        
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Type).HasColumnName("type");
        });
        
        modelBuilder.Entity<AccessKey>(entity =>
        {
            entity.ToTable("access_keys");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.CollectionId).HasColumnName("collection_id");
            entity.Property(e => e.Status).HasColumnName("status");
        });
        
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Code).HasColumnName("code");
        });
    }
}
