using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using parking_v6_to_v8_migration.Entities.v8;

namespace parking_v6_to_v8_migration.Configurations.v8;

public class AccessKeyCollectionConfiguration : IEntityTypeConfiguration<AccessKeyCollection>
{
    public void Configure(EntityTypeBuilder<AccessKeyCollection> builder)
    {
        builder.ToTable("access_key_collections");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Deleted).HasColumnName("deleted");
        builder.Property(x => x.VehicleType).HasColumnName("vehicle_type").HasConversion<string>();
        builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
    }
}