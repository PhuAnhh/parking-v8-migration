using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application_TV.Entities.v8;

namespace Application_TV.Configurations.v8;

public class EntryImageConfiguration : IEntityTypeConfiguration<EntryImage>
{
    public void Configure(EntityTypeBuilder<EntryImage> builder)
    {
        builder.ToTable("entry_images");
        builder.HasKey(x => new { x.ObjectKey, x.EntryId });
        builder.Property(x => x.ObjectKey).HasColumnName("object_key");
        builder.Property(x => x.EntryId).HasColumnName("entry_id");
        builder.Property(x => x.Type).HasColumnName("type");
    }
}