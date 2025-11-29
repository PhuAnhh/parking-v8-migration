using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application_v6.Entities.v8;

namespace Application_TV.Configurations.v8;

public class AccessKeyMetricConfiguration : IEntityTypeConfiguration<AccessKeyMetric>
{
    public void Configure(EntityTypeBuilder<AccessKeyMetric> builder)
    {
        builder.ToTable("access_key_metrics");
        builder.HasKey(n => new { n.AccessKeyId, n.RelatedAccessKeyId });
        builder.Property(x => x.AccessKeyId).HasColumnName("access_key_id");
        builder.Property(x => x.RelatedAccessKeyId).HasColumnName("related_access_key_id");
    }
}