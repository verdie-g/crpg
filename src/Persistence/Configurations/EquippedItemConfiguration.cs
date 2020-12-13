using Crpg.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class EquippedItemConfiguration : IEntityTypeConfiguration<EquippedItem>
    {
        public void Configure(EntityTypeBuilder<EquippedItem> builder)
        {
            builder.HasKey(ei => new { ei.CharacterId, ei.Slot });

            builder.HasOne(ei => ei.OwnedItem)
                .WithMany(oi => oi!.EquippedItems)
                .HasForeignKey(ei => new { ei.UserId, ei.ItemId });
        }
    }
}
