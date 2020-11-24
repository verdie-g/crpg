using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasIndex(i => i.MbId).IsUnique();
            builder.OwnsOne(i => i.Armor);
            builder.OwnsOne(i => i.Mount);
            builder.OwnsOne(i => i.PrimaryWeapon);
            builder.OwnsOne(i => i.SecondaryWeapon);
            builder.OwnsOne(i => i.TertiaryWeapon);

            // TODO: check value > 0
        }
    }
}