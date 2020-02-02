using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trpg.Domain.Entities;

namespace Trpg.Persistence.Configurations
{
    public class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.Property(c => c.Name).IsRequired();

            // TODO: check experience >= 0
            // TODO: check level >= 1
            // TODO: check equipment types
        }
    }
}