using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusBattleConfiguration : IEntityTypeConfiguration<StrategusBattle>
    {
        public void Configure(EntityTypeBuilder<StrategusBattle> builder)
        {
        }
    }
}
