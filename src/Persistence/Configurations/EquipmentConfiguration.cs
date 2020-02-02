using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trpg.Domain.Entities;

namespace Trpg.Persistence.Configurations
{
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.HasIndex(e => e.Name)
                .IsUnique();

            // TODO: check price > 0
        }
    }
}