using HarmonyLib;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.HarmonyPatches;

[HarmonyPatch(typeof(CustomBattleServer), "BeforeStartingNextBattle")]
public class BeforeStartingNextBattlePatch
{
    public static bool Prefix(CustomBattleServer __instance, GameLog[] gameLogs)
    {
        return false;
    }
}
