using Application_TV.Entities.v8.Resource.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v8.Resource;

public class CustomerCollectionConfiguration : IEntityTypeConfiguration<ResourceCustomerCollection>
{
    public void Configure(EntityTypeBuilder<ResourceCustomerCollection> builder)
    {
        builder.ToTable("customer_collections");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.ParentId).HasColumnName("parent_id");
        builder.Property(x => x.Deleted).HasColumnName("deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
    }
}