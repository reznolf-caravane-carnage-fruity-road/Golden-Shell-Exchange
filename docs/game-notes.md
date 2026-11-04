# CCFR game notes

Everything here was verified against the local install on 2026-11-03.

## Install layout

- Steam library root: `C:\Games\Steam` (+ `D:\SteamLibrary`)
- The game lives in a **nested** library:
  `D:\SteamLibrary\steamapps\steamapps\common\CCFR`
- Unity: **6000.3.8f1**, Mono backend, x64
- Godot-style `.DS_Store` present: the build was produced on macOS

Files that matter:

| File | Purpose |
|---|---|
| `Caravan Carnage Fruity Road_Data\Managed\Assembly-CSharp.dll` | all game code, unobfuscated |
| `..._Data\StreamingAssets\jsonData.json` | data-driven balance/content, read at runtime |
| `..._Data\data.unity3d` | scenes, sprites, audio, prefabs (~149 MB) |
| `%LOCALAPPDATA%Low\GonzoSSM\Caravan Carnage Fruity Road\playerInfo.dat` | save (BinaryFormatter) |
| `...\playerInfo.dat.bak` | previous save, written by `SafeSaveFile` |

## Save

`glov.Load()` / `glov.Save(bool ignoreSaveTimer)` use `SafeSaveFile`:

- `BinaryFormatter` serialization of `PlayerData`
- Atomic write: `.tmp` → rename → old file becomes `.bak`
- Recovery order: main → `.tmp` → `.bak`

Relevant fields: `glov.cc.playerGold`, `glov.cc.playerShells` (both `int`).

`jsonData.json` hash is computed by `My_Json_Signature.ComputeHash` and only
**printed** to the log; it is never compared. There is no integrity check.

## Main menu / UI

The main menu and the gameplay world share scene `level1`; panels are toggled via
`SetActive`. `StartMenuScript` is the main menu controller.

Key members:

```
StartMenuScript
  goldTXT, shellsTXT                    TMP counters
  cashBoxObj                            cash/shell counter root
  cash_pos_fly_to                       gold counter transform (animation target)
  cash_pos_fly_to_SHELLS                shell counter transform
  pauseSettingsBtn_obj                  settings button (a child of license_and_idle_btns)
  right_buttons, rightButtonsAnim       right-side menu buttons
  UpdateCashBox()                       refresh both counters + blink stars
  DoAspectRatioStuff()                  repositions pauseSettingsBtn_obj on aspect changes
  inputControllerNEW                    new Input System action map for the menu

glov.cc (glov)
  playerGold, playerShells
  loaded_save, hasFinishedBrutalTutorial
  zvukRef, urovRef, knopRef
  Save(bool ignoreSaveTimer)

urov
  SpawnFlyingCash(WhichIconsToShow, count, from, to)   IEnumerator, max 10
  SpawnFlyingNumber(text, from, to)                    flying "+cash" text
  flying_cash                                          FlyingCashObject prefab
  fx_holder_bullets_cash_explosions                    FX parent
```

`pauseSettingsBtn_obj` is hard-positioned by `DoAspectRatioStuff()`:

- PC: `localPosition = (5.86, 1.89, z)`
- non-PC: `(9.17, 1.89, z)`

That is why the mod hooks `DoAspectRatioStuff` and keeps the new button at
`+1.95` on X.

## Assets used by the mod

| Asset | Use |
|---|---|
| `golden_shell` (160×138) | button icon (replaces the gear on both the icon and its shadow) |
| `square_bubble` (9-sliced) | confirmation window background |
| `settings-button1` (1024×205) | confirmation buttons |
| `tmp_gonzoSSM` | TMP font (same as the game's cash counter) |

`FlyingCashObject.shellsImages[]` also contains shell sprites and is used as a
fallback.

## Data-driven JSON

`StreamingAssets/jsonData.json` is deserialized with Newtonsoft into
`JsonLoader.JsonData` (all values are quoted strings). It contains:

- `Rules` — ~140 global multipliers (economy, damage, spawn limits, timescale)
- `Enemy_*`, `Gun_*`, `Char_*`, vehicles, goodies, hires
- `Licenses`, `BOSS_STUFF`, `Special_Route_Values`, `GYPSY_BLACK_MARKET_TRADER`

`"isDEBUGGING_MODE": "true"` unlocks the in-game debug menu (god mode, cash,
mileage, forced routes). Note: debug mode also forces the UI language to Russian.

Steam updates / "Verify integrity" overwrite this file.

## Loader

BepInEx **5.4.23.6** (Doorstop 4.x) loads cleanly:

```
[Info : BepInEx] Running under Unity v6000.3.8.1867189
[Info : BepInEx] Chainloader ready
```

Plugins target `netstandard2.1` because Unity 6 assemblies reference
`netstandard 2.1`; targeting `netstandard2.0` fails with CS1705.
