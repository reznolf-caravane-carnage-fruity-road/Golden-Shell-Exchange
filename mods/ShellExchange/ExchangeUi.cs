using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CCFR.ShellExchange
{
    internal static class ExchangeUi
    {
        internal const int GoldPerShell = 1000;

        private const float ArtVisibleHeight = 1.10f;

        internal static float HorizontalOffset = 1.95f;

        private static StartMenuScript _menu;
        private static GameObject _button;
        private static Transform _source;
        private static bool _visualApplied;
        private static bool _busy;

        internal static GameObject ButtonObject => _button;

        internal static bool Busy
        {
            get => _busy;
            set => _busy = value;
        }

        public static void Ensure(StartMenuScript menu)
        {
            if (menu == null || menu.pauseSettingsBtn_obj == null || glov.cc == null)
            {
                return;
            }

            _menu = menu;

            if (_button != null && _source == menu.pauseSettingsBtn_obj.transform)
            {
                Sync();
                return;
            }

            Create(menu);
        }

        private static void Create(StartMenuScript menu)
        {
            Transform source = menu.pauseSettingsBtn_obj.transform;
            Transform parent = source.parent;
            if (parent == null)
            {
                return;
            }

            if (_button != null)
            {
                UnityEngine.Object.Destroy(_button);
            }

            _source = source;
            _visualApplied = false;
            _button = UnityEngine.Object.Instantiate(menu.pauseSettingsBtn_obj, parent);
            _button.name = "SHELL_EXCHANGE_BTN";

            ApplyVisual();
            StripInteractions();

            Transform plate = _button.transform.Find("rect_holder/BTN");
            if (plate != null)
            {
                var image = plate.GetComponent<Image>();
                if (image != null)
                {
                    image.raycastTarget = true;
                }

                var button = plate.GetComponent<Button>() ?? plate.gameObject.AddComponent<Button>();
                button.enabled = true;
                button.targetGraphic = image;
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(OnClicked);

                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 0.95f, 0.7f, 1f);
                colors.pressedColor = new Color(0.8f, 0.7f, 0.4f, 1f);
                button.colors = colors;
            }
            else
            {
                Plugin.Log.LogError("Could not find rect_holder/BTN in the cloned settings button");
            }

            BuildGlowAndBounce(plate);

            var driver = menu.gameObject.GetComponent<ShellExchangeDriver>();
            if (driver == null)
            {
                driver = menu.gameObject.AddComponent<ShellExchangeDriver>();
            }

            driver.Bind(menu, _button.transform, source);
            Sync();

            Plugin.Log.LogInfo($"Shell exchange button created next to the settings button (localPos={_button.transform.localPosition}, scale={_button.transform.localScale})");
            Plugin.Log.LogInfo($"Main menu balances: gold={glov.cc.playerGold} shells={glov.cc.playerShells}");
        }

        private static Sprite CurrentArt()
        {
            return SpriteUtil.ExchangeButton() ?? SpriteUtil.ShellSprite();
        }

        private static Vector2 ArtSize(Sprite art)
        {
            if (art == null)
            {
                return new Vector2(1.22f, 1.10f);
            }

            float fraction = SpriteUtil.ExchangeButtonHeightFraction;
            if (fraction <= 0.01f)
            {
                fraction = 1f;
            }

            float height = ArtVisibleHeight / fraction;
            float aspect = art.rect.height > 0f ? art.rect.width / art.rect.height : 1f;
            return new Vector2(height * aspect, height);
        }

        private static void ApplyVisual()
        {
            Sprite art = CurrentArt();
            if (art == null)
            {
                return;
            }

            Vector2 size = ArtSize(art);

            foreach (string path in new[] { "rect_holder/img (1)", "rect_holder/img (1)_1" })
            {
                Transform node = _button.transform.Find(path);
                if (node == null)
                {
                    continue;
                }

                var image = node.GetComponent<Image>();
                if (image == null)
                {
                    continue;
                }

                image.sprite = art;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;

                if (node is RectTransform rect)
                {
                    rect.sizeDelta = size;
                }
            }

            HorizontalOffset = 0.61f + size.x * 0.5f + 0.06f;
            _visualApplied = true;
        }

        private static void BuildGlowAndBounce(Transform plate)
        {
            Transform holder = _button.transform.Find("rect_holder");
            if (holder == null || plate == null)
            {
                return;
            }

            Transform previous = holder.Find("ShellGlow");
            if (previous != null)
            {
                UnityEngine.Object.Destroy(previous.gameObject);
            }

            RectTransform glowRt = UiFactory.Panel("ShellGlow", holder);
            Sprite art = CurrentArt();
            Vector2 size = ArtSize(art);
            glowRt.sizeDelta = size;
            glowRt.anchoredPosition = Vector2.zero;

            var glowImage = glowRt.gameObject.AddComponent<Image>();
            glowImage.sprite = art;
            glowImage.preserveAspect = false;
            glowImage.raycastTarget = false;
            glowImage.color = new Color(1f, 1f, 1f, 0f);

            if (plate is RectTransform plateRect)
            {
                plateRect.sizeDelta = new Vector2(size.x + 0.24f, size.y + 0.24f);
                plateRect.anchoredPosition = Vector2.zero;
            }

            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                glowRt.gameObject.layer = uiLayer;
            }

            var glowCanvas = glowRt.gameObject.AddComponent<Canvas>();
            glowCanvas.overrideSorting = true;
            glowCanvas.sortingLayerName = "GUI";
            glowCanvas.sortingOrder = 2;

            var bounce = plate.GetComponent<ClickBounce>() ?? plate.gameObject.AddComponent<ClickBounce>();
            bounce.Bind(holder, glowImage);
        }

        private static void StripInteractions()
        {
            foreach (Selectable selectable in _button.GetComponentsInChildren<Selectable>(true))
            {
                selectable.enabled = false;
            }

            foreach (EventTrigger trigger in _button.GetComponentsInChildren<EventTrigger>(true))
            {
                UnityEngine.Object.Destroy(trigger);
            }

            foreach (Graphic graphic in _button.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }
        }

        private static void Sync()
        {
            if (_button == null || _source == null)
            {
                return;
            }

            if (!_visualApplied)
            {
                ApplyVisual();
            }

            Vector3 p = _source.localPosition;
            _button.transform.localPosition = new Vector3(p.x + HorizontalOffset, p.y, p.z);
            _button.transform.localRotation = _source.localRotation;
            _button.transform.localScale = _source.localScale;
        }

        private static void OnClicked()
        {
            if (_busy || _menu == null || glov.cc == null)
            {
                return;
            }

            glov.cc.zvukRef.Play_MenuClick();

            int shells = glov.cc.playerShells;
            if (shells <= 0)
            {
                FXSpawner.SaySomethingOnMsgBlob("No golden shells to exchange", 1.2f, knop.MsgImgNames.none, false, 0);
                return;
            }

            long requested = (long)shells * GoldPerShell;
            long headroom = Int32.MaxValue - (long)glov.cc.playerGold;
            int gain = (int)Math.Min(requested, Math.Max(0L, headroom));

            _busy = true;
            _menu.StartCoroutine(ShowConfirmDelayed(shells, gain));
        }

        private static IEnumerator ShowConfirmDelayed(int shells, int gain)
        {
            // Let the press glow / bounce play before the dialog covers the button.
            yield return new WaitForSecondsRealtime(0.14f);

            if (_menu == null || glov.cc == null)
            {
                _busy = false;
                yield break;
            }

            ConfirmDialog.Show(_menu, shells, gain, confirmed =>
            {
                if (confirmed && _menu != null)
                {
                    _menu.StartCoroutine(ExchangeFlow.Run(_menu, shells, gain));
                }
                else
                {
                    _busy = false;
                }
            });
        }
    }

    internal sealed class ShellExchangeDriver : MonoBehaviour
    {
        private StartMenuScript _menu;
        private Transform _button;
        private Transform _source;

        public void Bind(StartMenuScript menu, Transform button, Transform source)
        {
            _menu = menu;
            _button = button;
            _source = source;
        }

        private void LateUpdate()
        {
            if (_menu == null || _button == null || _source == null)
            {
                return;
            }

            bool visible = _menu.isActiveAndEnabled
                && _menu.gameObject.activeInHierarchy
                && _source.gameObject.activeInHierarchy;

            if (_button.gameObject.activeSelf != visible)
            {
                _button.gameObject.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            Vector3 p = _source.localPosition;
            _button.localPosition = new Vector3(p.x + ExchangeUi.HorizontalOffset, p.y, p.z);
            _button.localRotation = _source.localRotation;
            _button.localScale = _source.localScale;
        }
    }
}
