namespace Crpg.Domain.Entities
{
    public class ItemWeaponComponent
    {
        public int Accuracy { get; set; }
        public int MissileSpeed { get; set; }
        public int StackAmount { get; set; }
        public int Length { get; set; }
        public int Handling { get; set; }
        public int BodyArmor { get; set; }
        public WeaponFlags Flags { get; set; }

        public int ThrustDamage { get; set; }
        public DamageType ThrustDamageType { get; set; }
        public int ThrustSpeed { get; set; }

        public int SwingDamage { get; set; }
        public DamageType SwingDamageType { get; set; }
        public int SwingSpeed { get; set; }
    }
}