using Crpg.Domain.Entities.Heroes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class HeroItemConfiguration : IEntityTypeConfiguration<HeroItem>
    {
        public void Configure(EntityTypeBuilder<HeroItem> builder)
        {
            builder.HasKey(oi => new { UserId = oi.HeroId, oi.ItemId });
        }
    }
}
