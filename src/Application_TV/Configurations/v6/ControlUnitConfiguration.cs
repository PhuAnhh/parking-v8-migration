using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v6;

public class ControlUnitConfiguration : IEntityTypeConfiguration<ControlUnit>
{
    public void Configure(EntityTypeBuilder<ControlUnit> builder)
    {
        builder.ToTable("ControlUnit");
        builder.HasKey(n => n.Id);
        builder.HasQueryFilter(x => !x.Deleted);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Code).HasColumnName("Code");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.ComputerId).HasColumnName("ComputerId");
        builder.Property(x => x.Enabled).HasColumnName("Enabled");
        builder.Property(x => x.Deleted).HasColumnName("Deleted");
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc");
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc");
    }
}