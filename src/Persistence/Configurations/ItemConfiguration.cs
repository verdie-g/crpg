using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Crpg.Domain.Entities;

namespace Crpg.Persistence.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasIndex(i => i.MbId).IsUnique();

            // TODO: check price > 0
        }
    }
}