# CCFR — Golden Shell Exchange

A BepInEx mod for **Caravan Carnage: Fruity Road** (Steam app `3330940`).

It adds a golden-shell button to the right of the Settings button on the main menu.
Clicking it opens a confirmation window, then converts **all** of your golden shells
into cash at **1 shell = $1,000**, with a flying-shell animation and a rolling
cash counter.

```
[ ⚙ Settings ]  [ 🐚 Exchange ]
                     │
                     ▼
        ┌─────────────────────────────┐
        │  EXCHANGE GOLDEN SHELLS?    │
        │  Convert 438 golden shells  │
        │  into $438,000 cash?        │
        │  1 golden shell = $1,000    │
        │    [ EXCHANGE ] [ CANCEL ]  │
        └─────────────────────────────┘
```

Conversions are saved through the game's own save routine (`glov.Save`) and are
never applied outside the main menu.

## Requirements

- Caravan Carnage: Fruity Road (Unity 6000.3.8f1, Mono)
- [BepInEx 5.4.23.6 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6)
  extracted into the game folder

## Install (players)

1. Install BepInEx 5.4.23.6 into the game folder (see `scripts/install-bepinex.ps1`).
2. Copy `CCFR.ShellExchange.dll` into:

   ```
   <game>\BepInEx\plugins\CCFR.ShellExchange.dll
   ```

3. Launch the game through Steam. The button appears next to Settings.

If the settings button is hidden (brutal tutorial not finished), the exchange
button is hidden too.

## Build from source

Requires a .NET SDK (tested with 8.0 and 9.0).

```powershell
dotnet build mods/ShellExchange/ShellExchange.csproj -c Release
```

The game path defaults to
`D:\SteamLibrary\steamapps\steamapps\common\CCFR`. Override it with:

```powershell
dotnet build mods/ShellExchange/ShellExchange.csproj -c Release -p:GameDir="C:\path\to\game"
```

Or use the helper scripts:

```powershell
pwsh scripts/build.ps1
pwsh scripts/deploy.ps1
```

## Safety

- Back up your save before trying any mod:

  ```powershell
  pwsh scripts/backup.ps1
  ```

  Save location:
  `%USERPROFILE%\AppData\LocalLow\GonzoSSM\Caravan Carnage Fruity Road\`

- The mod only writes `playerGold`, `playerShells` and then calls the game's
  `Save(ignoreSaveTimer: true)`. It never edits the save binary directly.
- The game has Steam Cloud enabled for its save. If a cloud copy ever wins over
  your local one, restore from `backups/`.

## How it works (short version)
| Piece | Game API used |
|---|---|
| Button placement | clone of `StartMenuScript.pauseSettingsBtn_obj`, offset +1.95 units |
| Icon | `golden_shell` sprite |
| Click handler | the cloned button's `BTN` plate |
| Balance fields | `glov.cc.playerGold`, `glov.cc.playerShells` |
| Animation | `urov.SpawnFlyingCash(WhichIconsToShow.shells, …)`, `urov.SpawnFlyingNumber(…)` |
| Sounds | `zvukRef.Play_MenuClick`, `Play_PickUp_Shell_Sound`, `Play_PickUp_Coin_Sound` |
| Save | `glov.cc.Save(ignoreSaveTimer: true)` |
| Hook | Harmony postfix on `StartMenuScript.DoAspectRatioStuff` / `OnEnable` |

More detail in [`docs/game-notes.md`](docs/game-notes.md) and
[`docs/ui-map.md`](docs/ui-map.md).

## Asset copies

Extracted game art lives in `icons/`:

- `icons/shells/` — all shell sprites, including `golden_shell.png` (used by the mod)
- `icons/money/` — all money sprites, including `moneyicon.png` (used by the mod)

See `icons/README.md` for the full list.

## Developer self-test

Set the environment variable before launching to run the whole flow automatically
(converts your current shells and saves; restore a backup afterwards):

```powershell
$env:CCFR_SHELLEXCHANGE_SELFTEST = "1"
```

The log is at `<game>\BepInEx\LogOutput.log`.

## Known limitations

- The confirmation window is built at runtime from the game's own sprites/fonts.
  It is not in the game's original style sheets, so tiny visual differences are
  expected.
- Pressing Escape while the confirmation is open does nothing; use Cancel or
  click outside the window.
- A game update that renames `StartMenuScript`, `glov` or `urov` members will
  break the mod (it will log an error and simply not add the button).
