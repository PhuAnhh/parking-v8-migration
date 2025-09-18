using parking_v6_to_v8_migration.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace parking_v6_to_v8_migration.Configurations.v6;

public class EventInConfiguration : IEntityTypeConfiguration<EventIn>
{
    public void Configure(EntityTypeBuilder<EventIn> builder)
    {
        builder.ToTable("EventIn");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId");
        builder.Property(x => x.LaneId).HasColumnName("LaneId");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.TotalPaid).HasColumnName("TotalPaid");
        builder.Property(x => x.Note).HasColumnName("Note");
        builder.Property(x => x.Status).HasColumnName("Status").HasConversion<string>();
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
    }
}