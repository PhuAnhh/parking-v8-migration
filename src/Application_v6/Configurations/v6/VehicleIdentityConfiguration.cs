using Application_v6.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6;

public class VehicleIdentityConfiguration : IEntityTypeConfiguration<VehicleIdentity>
{
    public void Configure(EntityTypeBuilder<VehicleIdentity> builder)
    {
        builder.ToTable("VehicleIdentities");
        builder.HasKey(x => new { x.VehicleId, x.IdentityId });
        builder.Property(x => x.VehicleId).HasColumnName("VehicleId");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
    }
}