using System;
using System.Collections;
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

            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                canvasGo.layer = uiLayer;
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            Camera guiCamera = glov.cc != null ? glov.cc.gui_camera_ref : null;

            if (guiCamera != null)
            {
                // Render through the game's GUI camera on a sorting layer above the UI
                // but below the custom mouse cursor (sorting layer "Cursor").
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = guiCamera;
                canvas.planeDistance = 1f;
                canvas.sortingLayerName = "MSG BLOB";
                canvas.sortingOrder = 9000;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 30000;
            }

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
            RectTransform panel = UiFactory.Panel("Window", canvasGo.transform);
            panel.sizeDelta = new Vector2(900f, 540f);
            panel.anchoredPosition = Vector2.zero;
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = panelSprite;
            panelImage.type = panelSprite != null && panelSprite.border.sqrMagnitude > 0.01f
                ? Image.Type.Sliced
                : Image.Type.Simple;
            panelImage.color = new Color(1f, 0.82f, 0.42f, 1f);
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
                new Vector2(760f, 150f),
                new Vector2(0f, 55f));

            BuildRateRow(panel, menu, font, material);

            var runner = canvasGo.AddComponent<DelayRunner>();
            bool closing = false;

            void Choose(bool result)
            {
                if (closing)
                {
                    return;
                }

                closing = true;

                foreach (Button b in canvasGo.GetComponentsInChildren<Button>(true))
                {
                    b.interactable = false;
                }

                runner.Run(0.16f, () => Close(result));
            }

            UiFactory.Button(
                "Yes",
                panel,
                font,
                material,
                "EXCHANGE",
                new Vector2(-190f, -185f),
                new Vector2(340f, 96f),
                new Color(0.55f, 0.9f, 0.55f, 1f),
                () => Choose(true));

            UiFactory.Button(
                "No",
                panel,
                font,
                material,
                "CANCEL",
                new Vector2(190f, -185f),
                new Vector2(340f, 96f),
                new Color(0.95f, 0.5f, 0.45f, 1f),
                () => Choose(false));

            if (uiLayer >= 0)
            {
                SetLayerRecursively(canvasGo, uiLayer);
            }

            Plugin.Log.LogInfo(
                $"Confirm dialog built (panel='{(panelSprite != null ? panelSprite.name : "MISSING")}', " +
                $"canvasMode={canvas.renderMode} layer='{canvas.sortingLayerName}' order={canvas.sortingOrder} " +
                $"font='{(font != null ? font.name : "MISSING")}')");
        }

        private static void BuildRateRow(RectTransform panel, StartMenuScript menu, TMP_FontAsset font, Material material)
        {
            RectTransform row = UiFactory.Panel("Rate", panel);
            row.sizeDelta = new Vector2(760f, 70f);
            row.anchoredPosition = new Vector2(0f, -70f);

            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 14f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            AddIcon(row, SpriteUtil.ShellSprite(), 44f, 38f);
            AddText(row, font, material, "1", 50f);
            AddText(row, font, material, "=", 34f);
            AddIcon(row, CashIcon(menu), 42f, 42f);
            AddText(row, font, material, "1,000", 120f);

            Sprite shell = SpriteUtil.ShellSprite();
            Sprite cash = CashIcon(menu);
            Plugin.Log.LogInfo($"Rate row icons: shell='{(shell != null ? shell.name : "MISSING")}' cash='{(cash != null ? cash.name : "MISSING")}'");
        }

        private static void AddIcon(Transform parent, Sprite sprite, float width, float height)
        {
            RectTransform rt = UiFactory.Panel("Icon", parent);
            rt.sizeDelta = new Vector2(width, height);

            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static void AddText(Transform parent, TMP_FontAsset font, Material material, string text, float width)
        {
            TextMeshProUGUI tmp = UiFactory.Text(
                "Rate" + text,
                parent,
                font,
                material,
                text,
                32f,
                TextAlignmentOptions.Center,
                new Vector2(width, 60f),
                Vector2.zero);
            tmp.fontStyle = FontStyles.Bold;
        }

        private static Sprite CashIcon(StartMenuScript menu)
        {
            if (menu != null && menu.cashBoxObj != null)
            {
                Transform node = menu.cashBoxObj.transform.Find("holder/mover/GOLD/ICO/mover/cashICON");
                var renderer = node != null ? node.GetComponent<SpriteRenderer>() : null;
                if (renderer != null && renderer.sprite != null)
                {
                    return renderer.sprite;
                }
            }

            return SpriteUtil.Named("gameover_cash_icons_0") ?? SpriteUtil.Named("Gold cash_0");
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

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;

            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        internal static void CloseForTest(bool result)
        {
            Close(result);
        }

        private static void Close(bool result)
        {
            if (!_open)
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

    internal sealed class DelayRunner : MonoBehaviour
    {
        public void Run(float seconds, Action action)
        {
            StartCoroutine(RunRoutine(seconds, action));
        }

        private IEnumerator RunRoutine(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }
    }
}
