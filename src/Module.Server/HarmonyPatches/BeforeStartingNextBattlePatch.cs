using HarmonyLib;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.HarmonyPatches;

[HarmonyPatch(typeof(CustomBattleServer), "BeforeStartingNextBattle")]
public class BeforeStartingNextBattlePatch
{
#pragma warning disable SA1313
    public static bool Prefix(CustomBattleServer __instance, GameLog[] gameLogs)
#pragma warning restore SA1313
    {
        return false;
    }
}
