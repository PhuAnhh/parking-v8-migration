using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Parking;

public class EventOutFileConfiguration : IEntityTypeConfiguration<EventOutFile>
{
    public void Configure(EntityTypeBuilder<EventOutFile> builder)
    {
        builder.ToTable("EventOutFiles");
        builder.HasKey(x => new { x.EventOutId, x.FileId });
        builder.Property(x => x.EventOutId).HasColumnName("EventOutId");
        builder.Property(x => x.FileId).HasColumnName("FileId");
        builder.Property(x => x.ImageType).HasColumnName("ImageType");
    }
}