using System.Collections;
using UnityEngine;

namespace CCFR.ShellExchange
{
    internal static class ExchangeFlow
    {
        public static IEnumerator Run(StartMenuScript menu, int shells, int gain)
        {
            try
            {
                var cc = glov.cc;
                if (cc == null || menu == null)
                {
                    yield break;
                }

                int oldGold = cc.playerGold;
                long target = (long)oldGold + gain;
                int newGold = target > int.MaxValue ? int.MaxValue : (int)target;

                cc.playerShells = 0;
                cc.playerGold = newGold;

                PlayFlight(menu, shells, gain);

                const float duration = 0.9f;
                float t = 0f;

                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / duration);
                    k = 1f - Mathf.Pow(1f - k, 3f);

                    SetGold(menu, (int)Mathf.Lerp(oldGold, newGold, k), newGold);
                    SetShells(menu, (int)Mathf.Lerp(shells, 0, k));
                    yield return null;
                }

                SetGold(menu, newGold, newGold);
                SetShells(menu, 0);

                cc.zvukRef.Play_PickUp_Coin_Sound();
                FXSpawner.SaySomethingOnMsgBlob(
                    "+" + gain.ToString("n0") + " CASH",
                    1.2f,
                    knop.MsgImgNames.none,
                    false,
                    0);

                cc.Save(ignoreSaveTimer: true);
                menu.UpdateCashBox();

                Plugin.Log.LogInfo($"Exchanged {shells} shells for {gain} cash (gold {oldGold} -> {cc.playerGold})");
            }
            finally
            {
                ExchangeUi.Busy = false;
            }
        }

        private static void PlayFlight(StartMenuScript menu, int shells, int gain)
        {
            var cc = glov.cc;
            if (cc == null || cc.urovRef == null)
            {
                return;
            }

            GameObject from = ExchangeUi.ButtonObject;
            GameObject to = menu.cash_pos_fly_to != null ? menu.cash_pos_fly_to.gameObject : null;
            if (from == null || to == null)
            {
                return;
            }

            int icons = Mathf.Clamp(shells / 40 + 2, 3, 10);
            cc.urovRef.StartCoroutine(
                cc.urovRef.SpawnFlyingCash(FlyingCashObject.WhichIconsToShow.shells, icons, from, to));
            cc.urovRef.SpawnFlyingNumber("<sprite=0>" + gain.ToString("n0"), from, to);
            cc.zvukRef.Play_PickUp_Shell_Sound();
        }

        private static void SetGold(StartMenuScript menu, int shown, int finalValue)
        {
            if (menu.goldTXT == null)
            {
                return;
            }

            menu.goldTXT.text = finalValue >= 1000000
                ? (shown * 1e-6f).ToString("0.0") + "M"
                : shown.ToString("n0");
        }

        private static void SetShells(StartMenuScript menu, int shown)
        {
            if (menu.shellsTXT == null)
            {
                return;
            }

            menu.shellsTXT.text = shown.ToString("n0");
        }
    }
}
