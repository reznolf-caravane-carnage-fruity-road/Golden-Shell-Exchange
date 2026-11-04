# Main menu UI map (scene `level1`)

Coordinates below are serialized `localPosition` values from the scene. At
runtime `StartMenuScript.DoAspectRatioStuff()` overrides the settings button
position (PC: `x = 5.86`, `y = 1.89`).

```
knop
└── ### Start_Menu                       (StartMenuScript lives here)
    ├── license_and_idle_btns            localPos (-17.0833, -5)
    │   ├── PAUSE_BTN                    localPos (5.86, 1.89) on PC   <- settings
    │   │   └── rect_holder              RectTransform 1.22 x 1.10, pos (-0.81,-0.72)
    │   │       ├── img (1)              Image sprite="settings" (gear)
    │   │       ├── tap_pos_PAUSE        gamepad hint anchor
    │   │       ├── BTN                  RectTransform 1.66 x 1.66, transparent, Button
    │   │       └── img (1)_1            Image sprite="settings", black 45.5% shadow
    │   ├── license_holder               localPos (2.264, 1.162), scale 0.445
    │   ├── idle_btn
    │   └── DEBUGGING_STUFF              localPos (20.22, 0) - debug mode only
    ├── CASHBOX                          localPos (-17.0833, +5)
    │   └── holder / mover
    │       ├── cash_pos_fly_to          gold counter anchor (animation target)
    │       │   └── cash_pos_fly_to_SHELLS
    │       ├── GOLD / mover / goldTXT   TMP, font tmp_gonzoSSM, size 35
    │       └── SHELLS / mover / shellsTXT
    └── RIGHT_BUTTONS                    localPos (4.5833, -5)
```

## Why the mod button is placed at +1.95 on X

- The gear icon is 1.22 units wide; the click plate is 1.66.
- `license_holder` and `idle_btn` sit to the **left** (x ≤ 2.26).
- `DEBUGGING_STUFF` starts at x = 20.22.
- Therefore `settings.x + 1.95` leaves a clean gap on the right with no overlap.

Runtime positions observed in the log:

```
settings: localPos=(5.86, 1.89, 0.00)
mod:      localPos=(7.81, 1.89, 0.00)
```

## Click path

The cloned button keeps the visual hierarchy but:

1. every `Selectable` in the clone is disabled,
2. every `Graphic.raycastTarget` is turned off,
3. the original `BTN` plate is re-enabled as our click target and all of its
   `onClick` listeners are cleared before adding the exchange handler.

Because the plate is transparent (`Image` sprite null, alpha 0), Unity still
raycasts its rect, so the whole shell icon is clickable.

## Confirmation window

Built at runtime on a `ScreenSpaceOverlay` canvas (`sortingOrder = 30000`,
CanvasScaler 1920×1080):

```
ShellExchangeConfirm (Canvas)
├── Dim            full-screen black 70%, click = Cancel
└── Window         900 x 540, sprite "square_bubble" (Sliced)
    ├── Title      "EXCHANGE GOLDEN SHELLS?"      46pt
    ├── Body       "Convert N golden shells ..."  38pt
    ├── Rate       "1 golden shell = $1,000"      28pt
    ├── Yes        340 x 96, "settings-button1", "EXCHANGE"
    └── No         340 x 96, "settings-button1", "CANCEL"
```

Font/material are borrowed from `StartMenuScript.goldTXT` (`tmp_gonzoSSM`).

While open, `StartMenuScript.inputControllerNEW` is disabled so the game's
Escape/Pause action cannot open the settings menu behind the dialog. It is
re-enabled on close.
