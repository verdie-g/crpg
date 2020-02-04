using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trpg.Domain.Entities;

namespace Trpg.Persistence.Configurations
{
    public class UserEquipmentConfiguration : IEntityTypeConfiguration<UserEquipment>
    {
        public void Configure(EntityTypeBuilder<UserEquipment> builder)
        {
            builder.HasKey(t => new {t.UserId, t.EquipmentId});

            builder
                .HasOne(ue => ue.User).WithMany(u => u.UserEquipments)
                .HasForeignKey(ue => ue.UserId);

            builder
                .HasOne(ue => ue.Equipment).WithMany(u => u.UserEquipments)
                .HasForeignKey(ue => ue.EquipmentId);
        }
    }
}