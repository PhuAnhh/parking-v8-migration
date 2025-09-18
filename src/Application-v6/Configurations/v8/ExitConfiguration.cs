using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using parking_v6_to_v8_migration.Entities.v8;

namespace parking_v6_to_v8_migration.Configurations.v8;

public class ExitConfiguration : IEntityTypeConfiguration<Exit>
{
    public void Configure(EntityTypeBuilder<Exit> builder)
    {
        builder.ToTable("exits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.EntryId).HasColumnName("entry_id");
        builder.Property(x => x.AccessKeyId).HasColumnName("access_key_id");
        builder.Property(x => x.PlateNumber).HasColumnName("plate_number");
        builder.Property(x => x.DeviceId).HasColumnName("device_id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.Amount).HasColumnName("amount");
        builder.Property(x => x.DiscountAmount).HasColumnName("discount_amount");
        builder.Property(x => x.Note).HasColumnName("note");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.Deleted).HasColumnName("deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
    }
}