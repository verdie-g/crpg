using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace Crpg.Module.HarmonyPatches;

/// <summary>
/// Because some bugs are fixed too slow by TaleWorlds, they are patched here using Harmony. It is the only acceptable
/// use of this library in this project. The patches should be removed as soon as TaleWorlds fixed the bugs.
/// </summary>
internal static class BannerlordPatches
{
    public static void Apply()
    {
        Harmony harmony = new("BannerlordServerPatches");
        harmony.PatchAll();

        AddPrefix(harmony, typeof(MissionLobbyComponent), "SendPeerInformationsToPeer",
            BindingFlags.NonPublic | BindingFlags.Instance, typeof(SendPeerInformationsToPeerPatch),
            nameof(SendPeerInformationsToPeerPatch.Prefix));
        AddPrefix(harmony, typeof(MissionNetworkComponent), "SendSpawnedMissionObjectsToPeer",
            BindingFlags.NonPublic | BindingFlags.Instance, typeof(MissionNetworkComponentPatch),
            nameof(MissionNetworkComponentPatch.Prefix));
        AddPrefix(harmony, typeof(DedicatedCustomServerSubModule), "GetUnsentGameLogs", BindingFlags.NonPublic | BindingFlags.Instance, typeof(GetUnsentGameLogsPatch), nameof(GetUnsentGameLogsPatch.Prefix));
    }

    private static void AddPrefix(Harmony harmony, Type classToPatch, string functionToPatchName, BindingFlags flags, Type patchClass, string functionPatchName)
    {
        var functionToPatch = classToPatch.GetMethod(functionToPatchName, flags);
        var newHarmonyPatch = patchClass.GetMethod(functionPatchName);
        harmony.Patch(functionToPatch, prefix: new HarmonyMethod(newHarmonyPatch));
    }
}
