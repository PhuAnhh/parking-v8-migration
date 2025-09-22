using Application_v6.Entities.v6.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Device;

public class GateConfiguration : IEntityTypeConfiguration<Gate>
{
    public void Configure(EntityTypeBuilder<Gate> builder)
    {
        builder.ToTable("Gates");
        builder.HasKey(n => n.Id);
        builder.HasQueryFilter(x => !x.Deleted);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Code).HasColumnName("Code");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Enabled).HasColumnName("Enabled");
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
    }
}