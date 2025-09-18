using Application_v6.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6;

public class IdentityConfiguration : IEntityTypeConfiguration<Identity>
{
    public void Configure(EntityTypeBuilder<Identity> builder)
    {
        builder.ToTable("Identities");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Code).HasColumnName("Code");
        builder.Property(x => x.IdentityGroupId).HasColumnName("IdentityGroupId");
        builder.Property(x => x.Type).HasColumnName("Type").HasConversion<string>();
        builder.Property(x => x.Status).HasColumnName("Status").HasConversion<string>();
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime>();
    }
}