using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusSettlementItemConfiguration : IEntityTypeConfiguration<StrategusSettlementItem>
    {
        public void Configure(EntityTypeBuilder<StrategusSettlementItem> builder)
        {
            builder.HasKey(oi => new { oi.SettlementId, oi.ItemId });
        }
    }
}
