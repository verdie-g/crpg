using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class HeroConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.HasOne<User>(h => h.User!)
            .WithOne(u => u.Hero!)
            .HasForeignKey<Hero>(u => u.Id);
    }
}
