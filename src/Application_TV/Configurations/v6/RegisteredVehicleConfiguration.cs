using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v6;

public class RegisteredVehicleConfiguration : IEntityTypeConfiguration<RegisteredVehicle>
{
    public void Configure(EntityTypeBuilder<RegisteredVehicle> builder)
    {
        builder.ToTable("RegisteredVehicle");
        builder.HasKey(n => n.Id);
        builder.HasQueryFilter(x => !x.Deleted);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.VehicleTypeId).HasColumnName("VehicleTypeId");
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId");
        builder.Property(x => x.ExpireUtc).HasColumnName("ExpireUtc").HasConversion<DateTime?>();
        builder.Property(x => x.LastActivatedUtc).HasColumnName("LastActivatedUtc").HasConversion<DateTime?>();
        builder.Property(x => x.Enabled).HasColumnName("Enabled").HasConversion<bool>();
        builder.Property(x => x.Deleted).HasColumnName("Deleted").HasConversion<bool>();
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime?>();
    }
}