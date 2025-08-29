using Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.DbContexts.v8;

public class ResourceDbContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }
    
    public ResourceDbContext(DbContextOptions<ResourceDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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