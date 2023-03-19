using Crpg.Domain.Entities.Limitations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class CharacterLimitationsConfiguration : IEntityTypeConfiguration<CharacterLimitations>
{
    public void Configure(EntityTypeBuilder<CharacterLimitations> builder)
    {
        builder.HasKey(cl => cl.CharacterId);
    }
}
