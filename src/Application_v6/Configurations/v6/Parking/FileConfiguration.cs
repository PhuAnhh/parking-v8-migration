using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = Application_v6.Entities.v6.Parking.File;

namespace Application_v6.Configurations.v6.Parking;

public class FileConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        builder.ToTable("Files");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Bucket).HasColumnName("Bucket");
        builder.Property(x => x.ObjectKey).HasColumnName("ObjectKey");
        builder.Property(x => x.ContentType).HasColumnName("ContentType");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
    }
}