using Crpg.Domain.Entities.Clans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class ClanInvitationConfiguration : IEntityTypeConfiguration<ClanInvitation>
    {
        public void Configure(EntityTypeBuilder<ClanInvitation> builder)
        {
        }
    }
}
