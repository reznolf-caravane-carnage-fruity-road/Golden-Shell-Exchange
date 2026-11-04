using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CCFR.ShellExchange
{
    internal static class ConfirmDialog
    {
        private static GameObject _root;
        private static bool _open;
        private static Action<bool> _callback;
        private static StartMenuScript _menu;

        public static void Show(StartMenuScript menu, int shells, int gain, Action<bool> callback)
        {
            if (_open)
            {
                return;
            }

            _open = true;
            _menu = menu;
            _callback = callback;

            try
            {
                Build(menu, shells, gain);

                if (menu.inputControllerNEW != null)
                {
                    menu.inputControllerNEW.Disable();
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Confirm dialog failed to build: " + e);
                Close(false);
            }
        }

        private static void Build(StartMenuScript menu, int shells, int gain)
        {
            var canvasGo = new GameObject(
                "ShellExchangeConfirm",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            _root = canvasGo;

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform dim = UiFactory.Panel("Dim", canvasGo.transform);
            UiFactory.Stretch(dim);
            var dimImage = dim.gameObject.AddComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, 0.7f);
            dimImage.raycastTarget = true;
            var dimButton = dim.gameObject.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.targetGraphic = dimImage;
            dimButton.onClick.AddListener(() => Close(false));

            Sprite panelSprite = SpriteUtil.Named("square_bubble");
            RectTransform panel = UiFactory.Panel("Window", canvasGo.transform);            panel.sizeDelta = new Vector2(900f, 540f);
            panel.anchoredPosition = Vector2.zero;
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = panelSprite;
            panelImage.type = panelSprite != null && panelSprite.border.sqrMagnitude > 0.01f
                ? Image.Type.Sliced
                : Image.Type.Simple;
            panelImage.color = Color.white;
            panelImage.raycastTarget = true;

            TMP_FontAsset font = menu.goldTXT != null ? menu.goldTXT.font : null;
            Material material = BuildTextMaterial(
                font,
                menu.goldTXT != null ? menu.goldTXT.fontSharedMaterial : null);

            UiFactory.Text(
                "Title",
                panel,
                font,
                material,
                "EXCHANGE GOLDEN SHELLS?",
                46f,
                TextAlignmentOptions.Center,
                new Vector2(790f, 90f),
                new Vector2(0f, 175f));

            UiFactory.Text(
                "Body",
                panel,
                font,
                material,
                $"Convert {shells:n0} golden shells\ninto ${gain:n0} cash?",
                38f,
                TextAlignmentOptions.Center,
                new Vector2(760f, 160f),
                new Vector2(0f, 40f));

            UiFactory.Text(
                "Rate",
                panel,
                font,
                material,
                "1 golden shell = $1,000",
                28f,
                TextAlignmentOptions.Center,
                new Vector2(700f, 50f),
                new Vector2(0f, -60f));

            UiFactory.Button(
                "Yes",
                panel,
                font,
                material,
                "EXCHANGE",
                new Vector2(-190f, -175f),
                new Vector2(340f, 96f),
                new Color(0.55f, 0.9f, 0.55f, 1f),
                () => Close(true));

            UiFactory.Button(
                "No",
                panel,
                font,
                material,
                "CANCEL",
                new Vector2(190f, -175f),
                new Vector2(340f, 96f),
                new Color(0.95f, 0.55f, 0.5f, 1f),
                () => Close(false));

            Plugin.Log.LogInfo(
                $"Confirm dialog built (panel='{(panelSprite != null ? panelSprite.name : "MISSING")}', " +
                $"button='{(SpriteUtil.Named("settings-button1") != null ? "settings-button1" : "fallback")}', " +
                $"font='{(font != null ? font.name : "MISSING")}')");
        }

        private static Material BuildTextMaterial(TMP_FontAsset font, Material source)
        {
            Material baseMaterial = source != null ? source : (font != null ? font.material : null);
            if (baseMaterial == null)
            {
                return null;
            }

            var material = new Material(baseMaterial)
            {
                name = "CCFR_ShellExchange_Text"
            };

            material.SetColor("_FaceColor", Color.black);
            material.SetColor("_OutlineColor", Color.white);
            material.SetFloat("_OutlineWidth", 0.22f);
            material.SetFloat("_OutlineSoftness", 0f);
            material.EnableKeyword("OUTLINE_ON");

            Color outlineColor = material.GetColor("_OutlineColor");
            float outlineWidth = material.GetFloat("_OutlineWidth");
            bool outlineOn = material.IsKeywordEnabled("OUTLINE_ON");

            Plugin.Log.LogInfo(
                $"Dialog text material built: outlineColor={outlineColor} width={outlineWidth} outlineOn={outlineOn}");

            return material;
        }

        internal static void CloseForTest(bool result)
        {
            Close(result);
        }

        private static void Close(bool result)
        {            if (!_open)
            {
                return;
            }

            _open = false;
            Action<bool> callback = _callback;
            _callback = null;

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }

            _root = null;

            if (_menu != null && _menu.inputControllerNEW != null)
            {
                _menu.inputControllerNEW.Enable();
            }

            _menu = null;
            callback?.Invoke(result);
        }
    }
}
