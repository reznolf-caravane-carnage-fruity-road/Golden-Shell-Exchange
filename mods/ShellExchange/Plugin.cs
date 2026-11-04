using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CCFR.ShellExchange
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.reznolf.ccfr.shellexchange";
        public const string Name = "Golden Shell Exchange";
        public const string Version = "0.1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{Name} v{Version} loading");

            SelfTest.Enabled = System.Environment.GetEnvironmentVariable("CCFR_SHELLEXCHANGE_SELFTEST") == "1";

            try
            {
                var harmony = new Harmony(Guid);
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                foreach (MethodBase method in harmony.GetPatchedMethods())
                {
                    Log.LogInfo($"Patched: {method.DeclaringType?.FullName}.{method.Name}");
                }

                Log.LogInfo("Harmony patches applied");
            }
            catch (Exception e)
            {
                Log.LogError("Failed to apply Harmony patches: " + e);
            }
        }
    }
}
