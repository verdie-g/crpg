using System;
using HarmonyLib;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.HarmonyPatches
{
    public static class GetUnsentGameLogsPatch
    {
        // This method will run before DedicatedCustomServerSubModule.GetUnsentGameLogs()
        public static bool Prefix(DedicatedCustomServerSubModule __instance, ref GameLog[] __result)
        {
            // Check if GameLogger or GameLogs is null
            if (__instance.GameLogger == null || __instance.GameLogger.GameLogs == null)
            {
                // Return an empty array
                __result = new GameLog[0];
                // Don't run the original method
                return false;
            }

            // Run the original method
            return true;
        }
    }
}
