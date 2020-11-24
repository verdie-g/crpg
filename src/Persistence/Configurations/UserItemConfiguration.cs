using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class UserItemConfiguration : IEntityTypeConfiguration<UserItem>
    {
        public void Configure(EntityTypeBuilder<UserItem> builder)
        {
            builder.HasKey(t => new { t.UserId, t.ItemId });

            builder
                .HasOne(oi => oi!.User).WithMany(u => u!.OwnedItems)
                .HasForeignKey(oi => oi.UserId);

            builder
                .HasOne(oi => oi!.Item).WithMany(u => u!.UserItems)
                .HasForeignKey(oi => oi.ItemId);
        }
    }
}
