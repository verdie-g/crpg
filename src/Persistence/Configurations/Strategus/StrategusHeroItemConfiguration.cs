using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusHeroItemConfiguration : IEntityTypeConfiguration<StrategusHeroItem>
    {
        public void Configure(EntityTypeBuilder<StrategusHeroItem> builder)
        {
            builder.HasKey(oi => new { UserId = oi.HeroId, oi.ItemId });
        }
    }
}
