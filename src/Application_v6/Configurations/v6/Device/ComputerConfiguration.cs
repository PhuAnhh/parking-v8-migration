using Application_v6.Entities.v6.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_v6.Configurations.v6.Device;

public class ComputerConfiguration : IEntityTypeConfiguration<Computer>
{
    public void Configure(EntityTypeBuilder<Computer> builder)
    {
        builder.ToTable("Computers");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Enabled).HasColumnName("Enabled").HasConversion<bool>();
        builder.Property(x => x.Deleted).HasColumnName("Deleted").HasConversion<bool>();
        builder.Property(x => x.CreatedUtc).HasColumnName("CreatedUtc").HasConversion<DateTime>();
        builder.Property(x => x.UpdatedUtc).HasColumnName("UpdatedUtc").HasConversion<DateTime>();
    }
}