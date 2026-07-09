# SCM20260D Dev — UI Designer edition

This is a **complete, deployable copy** of the official `SCM20260D Dev` demo, with the first screen
ported to the **TinyCLR UI Designer** (`.tcui`). It behaves identically to the original on the device —
same launcher, same feature windows, same navigation — but the *System Information* screen is now
**designed visually** instead of hand-coded.

## How to deploy
1. Open `Demos.sln` in Visual Studio (2022 or 2026) with the TinyCLR extension installed.
2. Connect your SCM20260D Dev board.
3. Press **Deploy** (or F5). You'll see the exact same demo as before — tap the gear ("System
   Information") icon to see the ported screen.

The project is **self-contained**: the UI-Designer codegen is bundled in `Build\`, so it builds even
if your installed extension's codegen is older. (Nothing in `Build\` ships to the device — it's a
build-time tool only.)

## What was ported (and what wasn't)
Only the **static visual tree** of a screen ports to `.tcui`; **runtime behavior stays in code**.
See `Port_to_UI_Designer.md` in the repo root for the full method.

- **`Windows\SystemInfoContent.tcui`** — the 7 information lines of the System screen, designed
  visually (open it in VS to edit with the live preview). Its root is a `<Canvas>`, so the designer
  generates a **reusable control** (`partial class SystemInfoContent : Canvas`) — see below.
- **`Windows\SystemInfoContent.cs`** — code-behind: fills the 3 runtime values (device name, clock,
  demo version) after `InitializeComponent()`.
- **`Windows\SystemWindow.cs`** — now just *hosts* the designed content inside the demo's
  `ApplicationWindow` navigation and adds the shared TopBar chrome. Compare it with the original
  `SCM20260D Dev\Windows\SystemWindow.cs` to see the before/after.

Every other window (WiFi, SD, USB, CAN, Buzzer, RTC, UART, ADC, PWM, DAC, Camera, Color, Basic Test,
Template) is **unchanged** and works exactly as before. Those screens are dominated by runtime logic
(sensor loops, threads, live text) that isn't a static layout, so they stay in code. Their small
static intro portions can be ported the same way as `SystemWindow` on request.

## The "fragment" feature this port added
A `.tcui` whose root is a **panel** (`Canvas`/`Grid`/`StackPanel`/`DockPanel`) — rather than a
`Window` — now generates a **reusable control** you can `new` and drop anywhere a `UIElement` is
accepted. That's what lets a designed screen plug into a *custom* navigation model (like this demo's
`ApplicationWindow`) instead of being forced to be a top-level `Window`. It's the key enabler for
porting real apps, and it's documented in `Port_to_UI_Designer.md`.
