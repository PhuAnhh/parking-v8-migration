using Application_v6.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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