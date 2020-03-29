using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
        }
    }
}