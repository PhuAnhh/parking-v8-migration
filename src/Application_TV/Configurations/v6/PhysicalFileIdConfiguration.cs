using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application_TV.Configurations.v6;

public class PhysicalFileIdConfiguration : IEntityTypeConfiguration<PhysicalFile>
{
    public void Configure(EntityTypeBuilder<PhysicalFile> builder)
    {
        builder.ToTable("PhysicalFile");
        builder.HasKey(n => n.Id);
        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.FileKey).HasColumnName("FileKey");
    }
}