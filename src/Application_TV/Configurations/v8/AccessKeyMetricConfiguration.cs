using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application_TV.Entities.v8.Resource;

namespace Application_v6.Configurations.v8;

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