namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Various properties of a <see cref="ItemWeaponComponent"/>. Should be synchronized
/// with TaleWorlds.Core.WeaponFlags.
/// </summary>
[Flags]
public enum WeaponFlags : long
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
    CanHook = 0x2000000,
    CanKnockDown = 0x4000000,
    CanCrushThrough = 0x8000000,
    CanBlockRanged = 0x10000000,
    MissileWithPhysics = 0x20000000,
    MultiplePenetration = 0x40000000,
    LeavesTrail = 0x80000000,
    UseHandAsThrowBase = 0x100000000,
    AmmoBreaksOnBounceBack = 0x1000000000,
    AmmoCanBreakOnBounceBack = 0x2000000000,
    AmmoSticksWhenShot = 0x4000000000,
}
