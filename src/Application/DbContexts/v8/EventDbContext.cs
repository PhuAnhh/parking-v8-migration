using Application.Entities.v8;
using Microsoft.EntityFrameworkCore;

namespace Application.DbContexts.v8;

public class EventDbContext : DbContext
{
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<EntryImage> EntryImages { get; set; }
    public DbSet<Exit> Exits { get; set; }
    public DbSet<ExitImage> ExitImages { get; set; }

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessKey>(entity =>
        {
            entity.ToTable("access_keys");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.CollectionId).HasColumnName("collection_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Code).HasColumnName("code");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("devices");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Type).HasColumnName("type");
        });

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

        modelBuilder.Entity<EntryImage>(entity =>
        {
            entity.ToTable("entry_images");
            entity.HasKey(e => new { e.EntryId, e.ObjectKey });
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.ObjectKey).HasColumnName("object_key");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<Exit>(entity =>
        {
            entity.ToTable("exits");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.AccessKeyId).HasColumnName("access_key_id");
            entity.Property(e => e.PlateNumber).HasColumnName("plate_number");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
        });

        modelBuilder.Entity<ExitImage>(entity =>
        {
            entity.ToTable("exit_images");
            entity.HasKey(e => new { e.ExitId, e.ObjectKey });
            entity.Property(e => e.ExitId).HasColumnName("exit_id");
            entity.Property(e => e.ObjectKey).HasColumnName("object_key");
            entity.Property(e => e.Type).HasColumnName("type");
        });
    }
}