namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Various properties of an <see cref="Item"/>. Should be synchronized with TaleWorlds.Core.ItemFlags.
/// </summary>
[Flags]
public enum ItemFlags
{
    ForceAttachOffHandPrimaryItemBone = 0x100,
    ForceAttachOffHandSecondaryItemBone = 0x200,
    NotUsableByFemale = 0x400,
    NotUsableByMale = 0x800,
    DropOnWeaponChange = 0x1000,
    DropOnAnyAction = 0x2000,
    CannotBePickedUp = 0x4000,
    CanBePickedUpFromCorpse = 0x8000,
    QuickFadeOut = 0x10000,
    WoodenAttack = 0x20000,
    WoodenParry = 0x40000,
    HeldInOffHand = 0x80000,
    HasToBeHeldUp = 0x100000,
    UseTeamColor = 0x200000,
    Civilian = 0x400000,
    DoNotScaleBodyAccordingToWeaponLength = 0x800000,
    DoesNotHideChest = 0x1000000,
    NotStackable = 0x2000000,
}
