using Application_v6.Entities.v6.Parking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Parking;

public class EventInConfiguration : IEntityTypeConfiguration<EventIn>
{
    public void Configure(EntityTypeBuilder<EventIn> builder)
    {
        builder.ToTable("EventIn");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
        builder.Property(x => x.IdentityGroupId).HasColumnName("IdentityGroupId");
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId");
        builder.Property(x => x.LaneId).HasColumnName("LaneId");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.TotalPaid).HasColumnName("TotalPaid");
        builder.Property(x => x.Note).HasColumnName("Note");
        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
        builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
    }
}