using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class OwnedItemConfiguration : IEntityTypeConfiguration<OwnedItem>
    {
        public void Configure(EntityTypeBuilder<OwnedItem> builder)
        {
            builder.HasKey(t => new { t.UserId, t.ItemId });

            builder
                .HasOne(oi => oi!.User).WithMany(u => u!.OwnedItems)
                .HasForeignKey(oi => oi.UserId);

            builder
                .HasOne(oi => oi!.Item).WithMany(i => i!.OwnedItems)
                .HasForeignKey(oi => oi.ItemId);
        }
    }
}
