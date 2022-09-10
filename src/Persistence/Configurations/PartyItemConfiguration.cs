using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class PartyItemConfiguration : IEntityTypeConfiguration<PartyItem>
{
    public void Configure(EntityTypeBuilder<PartyItem> builder)
    {
        builder.HasKey(oi => new { UserId = oi.PartyId, oi.ItemId });
    }
}
