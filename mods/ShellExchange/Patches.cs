using HarmonyLib;
using UnityEngine;

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

    [HarmonyPatch(typeof(knop), "GoToSettings")]
    internal static class KnopEscapePatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            // While the exchange dialog is open (or shortly after it closes on
            // Escape), swallow the game's Escape/pause handler so the pause menu
            // cannot open behind or after the dialog.
            float sinceClose = Time.unscaledTime - ConfirmDialog.ClosedAt;
            return !(ConfirmDialog.IsOpen || sinceClose < 0.25f);
        }
    }
}
