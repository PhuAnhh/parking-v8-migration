using Application_v6.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.VehicleType).HasColumnName("VehicleType").HasConversion<string>();
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId");
        builder.Property(x => x.ExpireUtc).HasColumnName("ExpireUtc").HasConversion<DateTime?>();
        builder.Property(x => x.LastActivatedUtc).HasColumnName("LastActivatedUtc").HasConversion<DateTime?>();
        builder.Property(x => x.Deleted).HasColumnName("Deleted").HasConversion<bool>();
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime>();
    }
}