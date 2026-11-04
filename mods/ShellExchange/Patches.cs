using HarmonyLib;

namespace CCFR.ShellExchange
{
    [HarmonyPatch(typeof(StartMenuScript), nameof(StartMenuScript.DoAspectRatioStuff))]
    internal static class StartMenuPatch
    {
        [HarmonyPostfix]
        private static void Postfix(StartMenuScript __instance)
        {
            try
            {
                ExchangeUi.Ensure(__instance);
                SelfTest.MaybeRun(__instance);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("ExchangeUi.Ensure failed: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(StartMenuScript), "OnEnable")]
    internal static class StartMenuOnEnablePatch
    {
        [HarmonyPostfix]
        private static void Postfix(StartMenuScript __instance)
        {
            try
            {
                ExchangeUi.Ensure(__instance);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("ExchangeUi.Ensure (OnEnable) failed: " + e);
            }
        }
    }
}
