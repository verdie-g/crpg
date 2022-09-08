using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class EquippedItemConfiguration : IEntityTypeConfiguration<EquippedItem>
{
    public void Configure(EntityTypeBuilder<EquippedItem> builder)
    {
        builder.HasKey(ei => new { ei.CharacterId, ei.Slot });
    }
}
