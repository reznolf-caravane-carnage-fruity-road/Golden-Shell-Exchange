using System;
using UnityEngine;

namespace CCFR.ShellExchange
{
    internal static class SpriteUtil
    {
        private static Sprite _shell;
        private static bool _shellLookupDone;

        public static Sprite ShellSprite()
        {
            if (_shellLookupDone)
            {
                return _shell;
            }

            _shellLookupDone = true;
            _shell = FindShell();

            if (_shell != null)
            {
                Plugin.Log.LogInfo($"Shell sprite resolved: '{_shell.name}' ({_shell.rect.width}x{_shell.rect.height})");
            }
            else
            {
                Plugin.Log.LogWarning("Shell sprite NOT FOUND - button will keep the default artwork");
            }

            return _shell;
        }

        private static Sprite FindShell()
        {
            var cc = glov.cc;
            if (cc != null && cc.urovRef != null)
            {
                GameObject[] sources = { cc.urovRef.flying_cash, cc.urovRef.flying_shells };
                foreach (GameObject go in sources)
                {
                    if (go == null)
                    {
                        continue;
                    }

                    var flying = go.GetComponent<FlyingCashObject>();
                    Sprite[] sprites = flying != null ? flying.shellsImages : null;
                    if (sprites == null)
                    {
                        continue;
                    }

                    foreach (Sprite s in sprites)
                    {
                        if (s != null)
                        {
                            return s;
                        }
                    }
                }
            }

            Sprite fallback = null;

            foreach (Sprite s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s == null)
                {
                    continue;
                }

                string name = s.name.ToLowerInvariant();

                if (name == "golden_shell")
                {
                    return s;
                }

                if (fallback == null && (name.Contains("golden_shell") || name.Contains("gold shells")))
                {
                    fallback = s;
                }
            }

            return fallback;
        }

        public static Sprite Named(string name)
        {
            foreach (Sprite s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s != null && s.name == name)
                {
                    return s;
                }
            }

            return null;
        }
    }
}
