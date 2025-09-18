using parking_v6_to_v8_migration.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace parking_v6_to_v8_migration.Configurations.v6;

public class IdentityGroupConfiguration : IEntityTypeConfiguration<IdentityGroup>
{
    public void Configure(EntityTypeBuilder<IdentityGroup> builder)
    {
        builder.ToTable("IdentityGroups");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Code).HasColumnName("Code");
        builder.Property(x => x.VehicleType).HasColumnName("VehicleType").HasConversion<string>();
        builder.Property(x => x.Deleted).HasColumnName("Deleted").HasConversion<bool>();
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime>();
    }
}