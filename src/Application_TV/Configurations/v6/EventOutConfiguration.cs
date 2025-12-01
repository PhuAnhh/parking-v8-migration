using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v6;

public class EventOutConfiguration : IEntityTypeConfiguration<EventOut>
{
    public void Configure(EntityTypeBuilder<EventOut> builder)
    {
        builder.ToTable("EventOut");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.IdentityId).HasColumnName("IdentityId");
        builder.Property(x => x.LaneId).HasColumnName("LaneId");
        builder.Property(x => x.PlateNumber).HasColumnName("PlateNumber");
        builder.Property(x => x.PhysicalFileIds).HasColumnName("PhysicalFileIds");
        builder.Property(x => x.EventInIdentityId).HasColumnName("EventInIdentityId");
        builder.Property(x => x.EventInLaneId).HasColumnName("EventInLaneId");
        builder.Property(x => x.EventInPlateNumber).HasColumnName("EventInPlateNumber");
        builder.Property(x => x.EventInCreatedUtc).HasColumnName("EventInCreatedUtc");
        builder.Ignore(x => x.EventInCreatedBy);
        builder.Property(x => x.Discount).HasColumnName("Discount");
        builder.Property(x => x.Charge).HasColumnName("Charge");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
        builder.Ignore(x => x.CreatedBy);
        builder.Property(x => x.EventInPhysicalFileIds).HasColumnName("EventInPhysicalFileIds");
        builder.Property(x => x.IdentityGroupId).HasColumnName("IdentityGroupId");
        builder.Property(x => x.CustomerId).HasColumnName("CustomerId");
        builder.Property(x => x.EventInIdentityGroupId).HasColumnName("EventInIdentityGroupId");
    }
}