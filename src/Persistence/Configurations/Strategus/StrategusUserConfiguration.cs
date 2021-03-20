using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusHeroConfiguration : IEntityTypeConfiguration<StrategusHero>
    {
        public void Configure(EntityTypeBuilder<StrategusHero> builder)
        {
            builder.HasKey(u => u.UserId);
        }
    }
}
