using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CCFR.ShellExchange
{
    internal static class UiFactory
    {
        public static RectTransform Panel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static TextMeshProUGUI Text(
            string name,
            Transform parent,
            TMP_FontAsset font,
            Material material,
            string text,
            float size,
            TextAlignmentOptions alignment,
            Vector2 sizeDelta,
            Vector2 position)
        {
            RectTransform rt = Panel(name, parent);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = position;

            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null)
            {
                tmp.font = font;
            }

            if (material != null)
            {
                tmp.fontSharedMaterial = material;
            }

            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static Button Button(
            string name,
            Transform parent,
            TMP_FontAsset font,
            Material material,
            string label,
            Vector2 position,
            Vector2 size,
            Color tint,
            Action onClick)
        {
            RectTransform rt = Panel(name, parent);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;

            var image = rt.gameObject.AddComponent<Image>();
            Sprite sprite = SpriteUtil.Named("settings-button1") ?? SpriteUtil.Named("square_bubble");
            image.sprite = sprite;
            image.type = sprite != null && sprite.border.sqrMagnitude > 0.01f
                ? Image.Type.Sliced
                : Image.Type.Simple;
            image.color = tint;
            image.raycastTarget = true;

            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;

            // White flash overlay used as the "light up" part of the click bounce.
            RectTransform glow = Panel("Glow", rt);
            Stretch(glow);
            var glowImage = glow.gameObject.AddComponent<Image>();
            glowImage.sprite = sprite;
            glowImage.type = image.type;
            glowImage.color = new Color(1f, 1f, 1f, 0f);
            glowImage.raycastTarget = false;
            glow.SetAsFirstSibling();

            var bounce = rt.gameObject.AddComponent<ClickBounce>();
            bounce.Bind(rt, glowImage);

            TextMeshProUGUI txt = Text(
                name + "Label",
                rt,
                font,
                material,
                label,
                50f,
                TextAlignmentOptions.Center,
                new Vector2(size.x - 24f, size.y - 10f),
                Vector2.zero);
            txt.fontStyle = FontStyles.Bold;

            return button;
        }
    }
}
