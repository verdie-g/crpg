using Crpg.Domain.Entities.Settlements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class SettlementItemConfiguration : IEntityTypeConfiguration<SettlementItem>
{
    public void Configure(EntityTypeBuilder<SettlementItem> builder)
    {
        builder.HasKey(oi => new { oi.SettlementId, oi.ItemId });
    }
}
