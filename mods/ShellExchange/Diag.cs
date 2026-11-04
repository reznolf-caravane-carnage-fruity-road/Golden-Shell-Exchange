using System.Text;
using TMPro;
using UnityEngine;

namespace CCFR.ShellExchange
{
    internal static class Diag
    {
        private static bool _done;

        public static void Ui()
        {
            if (_done)
            {
                return;
            }

            _done = true;

            var sb = new StringBuilder();
            sb.AppendLine("===== UI DIAG =====");

            var cc = glov.cc;
            if (cc != null)
            {
                AppendCamera(sb, "gui_camera_ref", cc.gui_camera_ref);
                AppendCamera(sb, "cam_ui2", cc.knopRef != null ? cc.knopRef.cam_ui2 : null);

                CursorScript cursor = cc.cursorScriptRef;
                if (cursor != null)
                {
                    sb.AppendLine($"cursor go layer={cursor.gameObject.layer} active={cursor.gameObject.activeInHierarchy}");
                    foreach (Renderer r in cursor.GetComponentsInChildren<Renderer>(true))
                    {
                        sb.AppendLine($"  renderer {r.GetType().Name} layer={r.gameObject.layer} sortingLayer='{r.sortingLayerName}' order={r.sortingOrder} enabled={r.enabled}");
                    }
                }

                if (cc.knopRef != null && cc.knopRef.start_menu != null)
                {
                    AppendCanvas(sb, "start_menu parent canvas", cc.knopRef.start_menu.GetComponentInParent<Canvas>());
                    foreach (Canvas cv in cc.knopRef.start_menu.GetComponentsInChildren<Canvas>(true))
                    {
                        AppendCanvas(sb, "  child canvas " + cv.gameObject.name, cv);
                    }
                }

                if (cc.urovRef != null && cc.urovRef.flying_num_txt != null)
                {
                    var flyingTxt = cc.urovRef.flying_num_txt.GetComponent<TMP_Text>();
                    sb.AppendLine(
                        $"flying_num_txt font={(flyingTxt != null && flyingTxt.font != null ? flyingTxt.font.name : "null")} " +
                        $"spriteAsset={(flyingTxt != null && flyingTxt.spriteAsset != null ? flyingTxt.spriteAsset.name : "null")}");
                    AppendSprites(sb, flyingTxt != null ? flyingTxt.spriteAsset : null, "flying");
                }

                if (cc.knopRef != null && cc.knopRef.start_menu != null && cc.knopRef.start_menu.goldTXT != null)
                {
                    var goldTxt = cc.knopRef.start_menu.goldTXT;
                    sb.AppendLine(
                        $"goldTXT spriteAsset={(goldTxt.spriteAsset != null ? goldTxt.spriteAsset.name : "null")} " +
                        $"material={(goldTxt.fontSharedMaterial != null ? goldTxt.fontSharedMaterial.name : "null")}");
                    AppendSprites(sb, goldTxt.spriteAsset, "gold");
                }
            }

            foreach (SortingLayer layer in SortingLayer.layers)
            {
                sb.AppendLine($"sortingLayer '{layer.name}' value={layer.value} id={layer.id}");
            }

            TMP_SpriteAsset sprites = TMP_Settings.defaultSpriteAsset;
            sb.AppendLine($"TMP defaultSpriteAsset={(sprites != null ? sprites.name : "null")}");
            if (sprites != null)
            {
                int i = 0;
                foreach (TMP_SpriteCharacter ch in sprites.spriteCharacterTable)
                {
                    sb.AppendLine($"  sprite[{i}] name={ch.name} unicode={ch.unicode}");
                    i++;
                    if (i > 20)
                    {
                        break;
                    }
                }
            }

            Plugin.Log.LogInfo(sb.ToString());
        }

        private static void AppendSprites(StringBuilder sb, TMP_SpriteAsset asset, string label)
        {
            if (asset == null)
            {
                sb.AppendLine($"{label} spriteAsset: null");
                return;
            }

            int i = 0;
            foreach (TMP_SpriteCharacter ch in asset.spriteCharacterTable)
            {
                sb.AppendLine($"  {label} sprite[{i}] name={ch.name} unicode={ch.unicode}");
                i++;
                if (i > 12)
                {
                    break;
                }
            }
        }

        private static void AppendCamera(StringBuilder sb, string label, Camera cam)        {
            if (cam == null)
            {
                sb.AppendLine($"{label}: null");
                return;
            }

            sb.AppendLine($"{label}: '{cam.name}' depth={cam.depth} clear={cam.clearFlags} mask=0x{cam.cullingMask:X} enabled={cam.enabled} goLayer={cam.gameObject.layer}");
        }

        private static void AppendCanvas(StringBuilder sb, string label, Canvas canvas)
        {
            if (canvas == null)
            {
                sb.AppendLine($"{label}: null");
                return;
            }

            Canvas root = canvas.rootCanvas;
            sb.AppendLine(
                $"{label}: go='{canvas.gameObject.name}' layer={canvas.gameObject.layer} mode={canvas.renderMode} " +
                $"sortingLayer='{canvas.sortingLayerName}' order={canvas.sortingOrder} override={canvas.overrideSorting} " +
                $"worldCamera={(canvas.worldCamera != null ? canvas.worldCamera.name : "null")} " +
                $"root='{(root != null ? root.gameObject.name : "null")}'");
        }
    }
}
