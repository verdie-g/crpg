using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations.Strategus
{
    public class StrategusOwnedItemConfiguration : IEntityTypeConfiguration<StrategusOwnedItem>
    {
        public void Configure(EntityTypeBuilder<StrategusOwnedItem> builder)
        {
            builder.HasKey(oi => new { oi.UserId, oi.ItemId });
        }
    }
}
