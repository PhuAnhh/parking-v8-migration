using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application_v6.Entities.v8;

namespace Application_v6.Configurations.v8;

public class AccessKeyConfiguration : IEntityTypeConfiguration<AccessKey>
{
    public void Configure(EntityTypeBuilder<AccessKey> builder)
    {
        builder.ToTable("access_keys");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Type).HasColumnName("type").HasConversion<string>();
        builder.Property(x => x.CollectionId).HasColumnName("collection_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
        builder.Property(x => x.Deleted).HasColumnName("deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
    }
}