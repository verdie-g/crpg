using System;
using System.Diagnostics;
using System.Reflection;
using TaleWorlds.Core;

namespace Crpg.GameMod
{
    internal static class DebugUtils
    {
        #if DEBUG
        private const bool Enabled = true;
        #else
        private const bool Enabled = false;
        #endif

        /// <remarks>Shouldn't be used in hot path.</remarks>
        public static void Trace(string message = "")
        {
            if (!Enabled)
            {
                return;
            }

            MethodBase callingMethod = new StackTrace().GetFrame(1).GetMethod();
            Type callingClass = callingMethod.ReflectedType;

            string log = $"[{callingClass.Name}::{callingMethod.Name}] {message}";
            InformationManager.DisplayMessage(new InformationMessage(log));
        }
    }
}