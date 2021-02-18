using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusSettlementConfiguration : IEntityTypeConfiguration<StrategusSettlement>
    {
        public void Configure(EntityTypeBuilder<StrategusSettlement> builder)
        {
            builder
                .HasOne(s => s.Owner!)
                .WithMany(u => u.OwnedSettlements);
        }
    }
}
