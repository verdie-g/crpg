using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusHeroConfiguration : IEntityTypeConfiguration<StrategusHero>
    {
        public void Configure(EntityTypeBuilder<StrategusHero> builder)
        {
            builder.HasOne<User>(h => h.User!)
                .WithOne(u => u.StrategusHero!)
                .HasForeignKey<StrategusHero>(u => u.Id);
        }
    }
}
