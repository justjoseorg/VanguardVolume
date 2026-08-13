# Vanguard Volume

Windows companion service for controlling the default output's master volume and up to five active audio applications from the Corsair VANGUARD 96 macro keys.

## MVP behavior

Map G1-G6 to **F13-F18** in iCUE or Corsair Web Hub. G1 selects Windows master volume. G2-G6 receive stable assignments to active shared-mode audio applications. After selecting a target, the keyboard's volume dial adjusts that target and dial press toggles mute. Each macro-key press opens the taskbar volume mixer using the native `Win`+`Ctrl`+`V` shortcut, then scrolls to its per-app controls.

The VANGUARD display is deliberately not driven by this MVP: Corsair has not published a supported live framebuffer/LCD API. Windows Settings is used instead of a custom dynamic display transport.

## Run

```powershell
dotnet run --project .\VanguardVolume.App
```

Leave the app running in the notification area. Use its menu to refresh sessions, show the mapping window, or exit.

## Known hardware constraint

The first version suppresses all standard Windows volume media events while it is running, because a low-level hook cannot yet identify the originating keyboard reliably. Map the VANGUARD's dial to the normal media volume events. Future hardware work can use Raw Input to limit suppression to the keyboard.
