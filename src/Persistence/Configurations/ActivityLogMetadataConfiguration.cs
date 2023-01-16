using Crpg.Domain.Entities.ActivityLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ActivityLogMetadataConfiguration : IEntityTypeConfiguration<ActivityLogMetadata>
{
    public void Configure(EntityTypeBuilder<ActivityLogMetadata> builder)
    {
        builder.HasKey(m => new { m.ActivityLogId, m.Key });
    }
}
