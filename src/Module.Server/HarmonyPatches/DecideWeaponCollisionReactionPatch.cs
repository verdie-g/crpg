using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches
{
    [HarmonyPatch(typeof(Mission))]
    [HarmonyPatch("DecideWeaponCollisionReaction")]
    public static class DecideWeaponCollisionReactionPatch
    {
        public static void Postfix(ref MeleeCollisionReaction colReaction,
                            in MissionWeapon attackerWeapon,
                            in AttackCollisionData collisionData)
        {
            // Check if the weapon used for the attack is an axe
            var weaponClass = attackerWeapon.IsEmpty ?
                              WeaponClass.Undefined :
                              attackerWeapon.CurrentUsageItem.WeaponClass;

            bool isAxe = weaponClass == WeaponClass.OneHandedAxe ||
                         weaponClass == WeaponClass.TwoHandedAxe;

            // Check if the attack hit an enemy (not a shield or an obstacle)
            bool hitEnemy = collisionData.IsColliderAgent && !collisionData.AttackBlockedWithShield;

            // Modify the collision reaction if the conditions are met
            if (isAxe && hitEnemy && colReaction != MeleeCollisionReaction.SlicedThrough)
            {
                colReaction = MeleeCollisionReaction.SlicedThrough;
            }
        }
    }
}
