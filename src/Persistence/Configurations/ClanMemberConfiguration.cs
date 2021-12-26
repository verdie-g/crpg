using Crpg.Domain.Entities.Clans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ClanMemberConfiguration : IEntityTypeConfiguration<ClanMember>
{
    public void Configure(EntityTypeBuilder<ClanMember> builder)
    {
        builder.HasKey(cm => cm.UserId);
    }
}
