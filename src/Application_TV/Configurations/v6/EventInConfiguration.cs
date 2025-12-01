using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v6;

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
        builder.Property(x => x.IdentityGroupId).HasColumnName("IdentityGroupId");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.Paid).HasColumnName("Paid");
        builder.Property(x => x.Note).HasColumnName("Note");
        builder.Property(x => x.PhysicalFileIds).HasColumnName("PhysicalFileIds");
        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
        builder.Ignore(x => x.CreatedBy);
    }
}