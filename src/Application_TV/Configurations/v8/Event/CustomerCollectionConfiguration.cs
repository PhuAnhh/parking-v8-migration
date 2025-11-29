using Application_TV.Entities.v8.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v8.Event;

public class CustomerCollectionConfiguration : IEntityTypeConfiguration<EventCustomerCollection>
{
    public void Configure(EntityTypeBuilder<EventCustomerCollection> builder)
    {
        builder.ToTable("customer_collections");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Deleted).HasColumnName("deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
    }
}