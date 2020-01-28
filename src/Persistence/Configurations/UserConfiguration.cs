using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trpg.Domain.Entities;

namespace Trpg.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(e => e.SteamId).IsUnique();

            builder.Property(e => e.UserName)
                .IsRequired();

            builder.Property(e => e.Avatar)
                .IsRequired();

            builder.Property(e => e.AvatarMedium)
                .IsRequired();

            builder.Property(e => e.AvatarFull)
                .IsRequired();
        }
    }
}