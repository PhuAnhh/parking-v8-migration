using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Parking;

public class EventInFileConfiguration : IEntityTypeConfiguration<EventInFile>
{
    public void Configure(EntityTypeBuilder<EventInFile> builder)
    {
        builder.ToTable("EventInFiles");
        builder.HasKey(x => new { x.EventInId, x.FileId });
        builder.Property(x => x.EventInId).HasColumnName("EventInId");
        builder.Property(x => x.FileId).HasColumnName("FileId");
        builder.Property(x => x.ImageType).HasColumnName("ImageType");
    }
}