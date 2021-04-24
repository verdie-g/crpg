using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public record ItemWeaponComponentViewModel : IMapFrom<ItemWeaponComponent>
    {
        public WeaponClass Class { get; init; }
        public int Accuracy { get; init; }
        public int MissileSpeed { get; init; }
        public int StackAmount { get; init; }
        public int Length { get; init; }
        public float Balance { get; init; }
        public int Handling { get; init; }
        public int BodyArmor { get; init; }
        public WeaponFlags Flags { get; init; }

        public int ThrustDamage { get; init; }
        public DamageType ThrustDamageType { get; init; }
        public int ThrustSpeed { get; init; }

        public int SwingDamage { get; init; }
        public DamageType SwingDamageType { get; init; }
        public int SwingSpeed { get; init; }
    }
}
