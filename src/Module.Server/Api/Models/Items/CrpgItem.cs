namespace Crpg.Module.Api.Models.Items;

// Copy of Crpg.Application.Items.Model.ItemViewModel
internal class CrpgItem : CrpgItemCreation
{
    public int Id { get; set; }
    public int Price { get; set; }
}

internal class CrpgItemCreation
{
    public string TemplateMbId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CrpgCulture Culture { get; set; }
    public CrpgItemType Type { get; set; }
    public float Weight { get; set; }

    public CrpgItemArmorComponent? Armor { get; set; }
    public CrpgItemMountComponent? Mount { get; set; }
    public IList<CrpgItemWeaponComponent> Weapons { get; set; } = Array.Empty<CrpgItemWeaponComponent>();
}

// Copy of Crpg.Domain.Entities.Items.ItemType
internal enum CrpgItemType
{
    Undefined,
    HeadArmor,
    ShoulderArmor,
    BodyArmor,
    HandArmor,
    LegArmor,
    MountHarness,
    Mount,
    Shield,
    Bow,
    Crossbow,
    OneHandedWeapon,
    TwoHandedWeapon,
    Polearm,
    Thrown,
    Arrows,
    Bolts,
    Pistol,
    Musket,
    Bullets,
    Banner,
}

// Copy of Crpg.Application.Items.Models.ItemArmorComponentViewModel
internal class CrpgItemArmorComponent
{
    public int HeadArmor { get; set; }
    public int BodyArmor { get; set; }
    public int ArmArmor { get; set; }
    public int LegArmor { get; set; }
    public CrpgArmorMaterialType MaterialType { get; set; }
}

// Copy of Crpg.Domain.Entities.Items.ArmorMaterialType
internal enum CrpgArmorMaterialType
{
    Undefined,
    Cloth,
    Leather,
    Chainmail,
    Plate,
}

// Copy of Crpg.Application.Items.Models.ItemMountComponentViewModel
internal class CrpgItemMountComponent
{
    public int BodyLength { get; set; }
    public int ChargeDamage { get; set; }
    public int Maneuver { get; set; }
    public int Speed { get; set; }
    public int HitPoints { get; set; }
}

// Copy of Crpg.Application.Items.Models.ItemMountWeaponViewModel
internal class CrpgItemWeaponComponent
{
    public CrpgWeaponClass Class { get; set; }
    public int Accuracy { get; set; }
    public int MissileSpeed { get; set; }
    public int StackAmount { get; set; }
    public int Length { get; set; }
    public float Balance { get; set; }
    public int Handling { get; set; }
    public int BodyArmor { get; set; }
    public CrpgWeaponFlags Flags { get; set; }

    public int ThrustDamage { get; set; }
    public CrpgDamageType ThrustDamageType { get; set; }
    public int ThrustSpeed { get; set; }

    public int SwingDamage { get; set; }
    public CrpgDamageType SwingDamageType { get; set; }
    public int SwingSpeed { get; set; }
}

// Copy of Crpg.Domain.Entities.Items.DamageType
internal enum CrpgDamageType
{
    Undefined,
    Cut,
    Pierce,
    Blunt,
}

// Copy of Crpg.Domain.Entities.Items.WeaponFlags
[Flags]
internal enum CrpgWeaponFlags : long
{
    MeleeWeapon = 0x1,
    RangedWeapon = 0x2,
    FirearmAmmo = 0x4,
    NotUsableWithOneHand = 0x10,
    NotUsableWithTwoHand = 0x20,
    WideGrip = 0x40,
    AttachAmmoToVisual = 0x80,
    Consumable = 0x100,
    HasHitPoints = 0x200,
    HasString = 0x400,
    StringHeldByHand = 0xC00,
    UnloadWhenSheathed = 0x1000,
    AffectsArea = 0x2000,
    AffectsAreaBig = 0x4000,
    Burning = 0x8000,
    BonusAgainstShield = 0x10000,
    CanPenetrateShield = 0x20000,
    CantReloadOnHorseback = 0x40000,
    AutoReload = 0x80000,
    TwoHandIdleOnMount = 0x200000,
    NoBlood = 0x400000,
    PenaltyWithShield = 0x800000,
    CanDismount = 0x1000000,
    MissileWithPhysics = 0x2000000,
    MultiplePenetration = 0x4000000,
    CanKnockDown = 0x8000000,
    CanBlockRanged = 0x10000000,
    LeavesTrail = 0x20000000,
    CanCrushThrough = 0x40000000,
    UseHandAsThrowBase = 0x80000000,
    AmmoBreaksOnBounceBack = 0x100000000,
    AmmoCanBreakOnBounceBack = 0x200000000,
    AmmoSticksWhenShot = 0x400000000,
}

// Copy of Crpg.Domain.Entities.Items
internal enum CrpgWeaponClass
{
    Undefined,
    Dagger,
    OneHandedSword,
    TwoHandedSword,
    OneHandedAxe,
    TwoHandedAxe,
    Mace,
    Pick,
    TwoHandedMace,
    OneHandedPolearm,
    TwoHandedPolearm,
    LowGripPolearm,
    Arrow,
    Bolt,
    Cartridge,
    Bow,
    Crossbow,
    Stone,
    Boulder,
    ThrowingAxe,
    ThrowingKnife,
    Javelin,
    Pistol,
    Musket,
    SmallShield,
    LargeShield,
    Banner,
}
