using Crpg.Domain.Entities.Settlements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
    {
        public void Configure(EntityTypeBuilder<Settlement> builder)
        {
            builder
                .HasOne(s => s.Owner!)
                .WithMany(u => u.OwnedSettlements);

            builder.HasIndex(s => new { s.Region, s.Name }).IsUnique();
        }
    }
}
