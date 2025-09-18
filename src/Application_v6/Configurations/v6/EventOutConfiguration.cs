using Application_v6.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6;

public class EventOutConfiguration : IEntityTypeConfiguration<EventOut>
{
    public void Configure(EntityTypeBuilder<EventOut> builder)
    {
        builder.ToTable("EventOut");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
        builder.Property(x => x.LaneId).HasColumnName("LaneId");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.DiscountAmount).HasColumnName("DiscountAmount");
        builder.Property(x => x.Charge).HasColumnName("Charge").HasConversion<long>();
        builder.Property(x => x.Note).HasColumnName("Note");
        builder.Property(x => x.EventInId).HasColumnName("EventInId");
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
    }
}