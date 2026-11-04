using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CCFR.ShellExchange
{
    internal static class SpriteUtil
    {
        private static Sprite _shell;
        private static bool _shellLookupDone;
        private static Sprite _exchangeButton;

        public static Sprite ExchangeButton()
        {
            if (_exchangeButton != null)
            {
                return _exchangeButton;
            }

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("CCFR.ShellExchange.exchange_button.png"))
                {
                    if (stream == null)
                    {
                        Plugin.Log.LogWarning("Embedded exchange_button.png not found");
                        return null;
                    }

                    var data = new byte[stream.Length];
                    int read = 0;
                    while (read < data.Length)
                    {
                        int chunk = stream.Read(data, read, data.Length - read);
                        if (chunk <= 0)
                        {
                            break;
                        }

                        read += chunk;
                    }

                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                    {
                        name = "exchange_button"
                    };
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;

                    if (!texture.LoadImage(data))
                    {
                        Plugin.Log.LogError("Failed to decode embedded exchange_button.png");
                        return null;
                    }

                    _exchangeButton = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    _exchangeButton.name = "exchange_button";

                    Plugin.Log.LogInfo($"Custom exchange button sprite loaded ({texture.width}x{texture.height})");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Failed to load exchange_button.png: " + e);
            }

            return _exchangeButton;
        }

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
