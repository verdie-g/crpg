using Crpg.Domain.Entities.Clans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ClanConfiguration : IEntityTypeConfiguration<Clan>
{
    public void Configure(EntityTypeBuilder<Clan> builder)
    {
        builder.HasIndex(c => c.Tag).IsUnique();
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
