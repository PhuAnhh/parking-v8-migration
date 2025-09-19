using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application_v6.Entities.v8;

namespace Application_v6.Configurations.v8;

public class ExitImageConfiguration : IEntityTypeConfiguration<ExitImage>
{
    public void Configure(EntityTypeBuilder<ExitImage> builder)
    {
        builder.ToTable("exit_images");
        builder.HasKey(x => new { x.ExitId, x.ObjectKey });
        builder.Property(x => x.ExitId).HasColumnName("exit_id");
        builder.Property(x => x.ObjectKey).HasColumnName("object_key");
        builder.Property(x => x.Type).HasColumnName("type");
    }
}