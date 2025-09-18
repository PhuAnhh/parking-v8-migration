using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application_v6.Entities.v8;

namespace Application_v6.Configurations.v8;

public class CustomerCollectionConfiguration : IEntityTypeConfiguration<CustomerCollection>
{
    public void Configure(EntityTypeBuilder<CustomerCollection> builder)
    {
        builder.ToTable("customer_collections");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Deleted).HasColumnName("deleted");
        builder.Property(x => x.ParentId).HasColumnName("parent_id");
        builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
    }
}