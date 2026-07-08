# Car Wash Controller — UI Designer edition

A complete, deployable copy of the `Car Wash Controller` sample with **all five screens ported to the
TinyCLR UI Designer** (`.tcui`). It builds and behaves the same as the original; the difference is that
each screen's layout is now **designed visually** (`.tcui` + live preview) instead of hand-coded.

## How to deploy
1. Open `CarWashExample.sln` in Visual Studio (with the TinyCLR extension) and connect an SCM20260D board.
2. Hold the **LDR button** on power-up to start (same as the original — the LED blinks while waiting).
3. Press **Deploy / F5**. Flow: Select Service → Payment → Loading → Washing → End → back to Select.

The project is **self-contained**: the UI-Designer codegen is bundled in `Build\`, so it builds even if
your installed extension's codegen is older. Nothing in `Build\` ships to the device.

## What was ported
Each page is now a **`.tcui` fragment** (root is a `<Canvas>` → the designer generates a reusable
control, `partial class Page : Canvas`) plus a small code-behind for the runtime behavior. `Program.cs`
is unchanged — it still navigates with `page.Elements` (each page exposes `Elements => this`).

| Screen | `.tcui` (static layout) | Code-behind (runtime) |
|---|---|---|
| Select Service | buttons, prices, divider, prompt, logos, date placeholder | fills the date; populates the vehicle `ListBox` (custom highlightable rows); button handler |
| Payment | 3 labels, 3 text boxes, Back/Next | on-screen-keyboard font; Back/Next handlers (confirm `MessageBox`) |
| Loading | label + progress bar | `DispatcherTimer` advancing the bar, then navigate |
| Car Wash | label + progress bar | `DispatcherTimer` counting down, then navigate |
| End | checkbox + 3 radios + labels + Done | Done handler |

## Two things worth knowing
- **Design-time `Background`.** Each `.tcui` root has `Background="Teal"`. That's a **preview aid only**
  so the white text is visible while designing — the designer paints it, but codegen *ignores*
  `Background` on a panel root, so on the device the real blue→teal gradient (set on the `Window` in
  `Program.cs`) is used. Preview ≈ device.
- **The vehicle list** uses a custom `ListBoxItemHighlightable` (not a standard item), so its rows are
  added in code-behind; the `.tcui` just holds the empty, positioned `ListBox`. The original divider
  `Line` became a 1px `Rectangle` (codegen has no `Line`) — visually identical.

The original project under `Applications\Car Wash Controller` is untouched.
