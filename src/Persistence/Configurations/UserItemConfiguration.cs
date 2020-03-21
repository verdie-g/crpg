using Crpg.Domain.Entities;
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
                .HasOne(ui => ui!.User).WithMany(u => u!.UserItems)
                .HasForeignKey(ui => ui.UserId);

            builder
                .HasOne(ui => ui!.Item).WithMany(u => u!.UserItems)
                .HasForeignKey(ui => ui!.ItemId);
        }
    }
}