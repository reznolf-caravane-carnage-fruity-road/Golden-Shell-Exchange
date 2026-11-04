using System.Collections;
using UnityEngine;

namespace CCFR.ShellExchange
{
    internal static class SelfTest
    {
        internal static bool Enabled;
        private static bool _ran;

        public static void MaybeRun(StartMenuScript menu)
        {
            if (!Enabled || _ran || menu == null || glov.cc == null)
            {
                return;
            }

            _ran = true;
            menu.StartCoroutine(Run(menu));
        }

        private static IEnumerator Run(StartMenuScript menu)
        {
            yield return new WaitForSeconds(2f);

            if (glov.cc == null)
            {
                yield break;
            }

            int shells = glov.cc.playerShells;
            int gain = shells * ExchangeUi.GoldPerShell;

            if (System.Environment.GetEnvironmentVariable("CCFR_SHELLEXCHANGE_PREVIEW") == "1")
            {
                Diag.Ui();
                ConfirmDialog.Show(menu, shells, gain, ok => Plugin.Log.LogInfo($"[PREVIEW] closed={ok}"));

                float seconds = 1.5f;
                string overrideSeconds = System.Environment.GetEnvironmentVariable("CCFR_SHELLEXCHANGE_PREVIEW_SECONDS");
                if (!string.IsNullOrEmpty(overrideSeconds) && float.TryParse(overrideSeconds, out float parsed))
                {
                    seconds = parsed;
                }

                yield return new WaitForSecondsRealtime(seconds);
                ConfirmDialog.CloseForTest(false);
                yield break;
            }

            Diag.Ui();

            Plugin.Log.LogInfo($"[SELFTEST] before: gold={glov.cc.playerGold} shells={glov.cc.playerShells} gain={gain}");

            bool accepted = false;
            ConfirmDialog.Show(menu, shells, gain, ok => accepted = ok);
            yield return new WaitForSeconds(0.75f);
            bool rootExists = GameObject.Find("ShellExchangeConfirm") != null;
            Plugin.Log.LogInfo($"[SELFTEST] dialog open, forcing YES (root exists: {rootExists})");
            ConfirmDialog.CloseForTest(true);
            yield return new WaitForSeconds(0.25f);
            Plugin.Log.LogInfo($"[SELFTEST] callback result={accepted}");

            ExchangeUi.Busy = true;
            yield return menu.StartCoroutine(ExchangeFlow.Run(menu, shells, gain));

            Plugin.Log.LogInfo($"[SELFTEST] after: gold={glov.cc.playerGold} shells={glov.cc.playerShells}");
        }
    }
}
