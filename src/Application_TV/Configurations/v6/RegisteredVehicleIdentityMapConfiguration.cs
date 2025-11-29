using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v6;

public class RegisteredVehicleIdentityMapConfiguration : IEntityTypeConfiguration<RegisteredVehicleIdentityMap>
{
    public void Configure(EntityTypeBuilder<RegisteredVehicleIdentityMap> builder)
    {
        builder.ToTable("RegisteredVehicleIdentityMap");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.RegisteredVehicleId).HasColumnName("RegisteredVehicleId");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
    }
}