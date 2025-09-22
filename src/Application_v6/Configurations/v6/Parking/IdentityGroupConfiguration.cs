using Application_v6.Entities.v6.Parking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Parking;

public class IdentityGroupConfiguration : IEntityTypeConfiguration<IdentityGroup>
{
    public void Configure(EntityTypeBuilder<IdentityGroup> builder)
    {
        builder.ToTable("IdentityGroups");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Code).HasColumnName("Code");
        builder.Property(x => x.VehicleType).HasColumnName("VehicleType");
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
    }
}