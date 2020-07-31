using System;

namespace Crpg.Domain.Entities
{
    [Flags]
    public enum WeaponFlags : long
    {
        MeleeWeapon = 0x1,
        RangedWeapon = 0x2,
        WeaponMask = RangedWeapon | MeleeWeapon,
        FirearmAmmo = 0x4,
        NotUsableWithOneHand = 0x10,
        NotUsableWithTwoHand = 0x20,
        HandUsageMask = NotUsableWithTwoHand | NotUsableWithOneHand,
        WideGrip = 0x40,
        AttachAmmoToVisual = 0x80,
        Consumable = 0x100,
        HasHitPoints = 0x200,
        DataValueMask = HasHitPoints | Consumable,
        HasString = 0x400,
        StringHeldByHand = 0xC00,
        UnloadWhenSheathed = 0x1000,
        AffectsArea = 0x2000,
        Burning = 0x8000,
        BonusAgainstShield = 0x10000,
        CanPenetrateShield = 0x20000,
        CantReloadOnHorseback = 0x40000,
        AutoReload = 0x80000,
        CrushThrough = 0x100000,
        TwoHandIdleOnMount = 0x200000,
        PenaltyWithShield = 0x800000,
        MissileWithPhysics = 0x2000000,
        MultiplePenetration = 0x4000000,
        CanKnockDown = 0x8000000,
        CanBlockRanged = 0x10000000,
        LeavesTrail = 0x20000000,
        UseHandAsThrowBase = 0x80000000,
        AmmoBreaksOnBounceBack = 0x100000000,
        AmmoCanBreakOnBounceBack = 0x200000000,
        AmmoBreakOnBounceBackMask = AmmoCanBreakOnBounceBack | AmmoBreaksOnBounceBack,
        AmmoSticksWhenShot = 0x400000000,
    }
}