using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Parking;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.VehicleType).HasColumnName("VehicleType");
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId");
        builder.Property(x => x.ExpireUtc).HasColumnName("ExpireUtc");
        builder.Property(x => x.LastActivatedUtc).HasColumnName("LastActivatedUtc");
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
    }
}