using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for SystemInfoContent.tcui (the body of the System Information screen).
    //
    // The static 7-line layout is designed in SystemInfoContent.tcui and built by InitializeComponent()
    // (generated into SystemInfoContent.tcui.g.cs). This partial only fills the three runtime-computed
    // lines — exactly the split described in Port_to_UI_Designer.md: static tree in .tcui, dynamic values here.
    //
    // The .tcui root is a <Canvas>, so codegen makes this a reusable control (a Canvas), which SystemWindow
    // hosts inside the demo's ApplicationWindow navigation.
    public partial class SystemInfoContent : Canvas {
        private const string DemoVersion = "052126"; // May-21-2026 — last changed

        public SystemInfoContent() {
            InitializeComponent();

            this._deviceText.TextContent = "Device: " + DeviceInformation.DeviceName;
            this._clockText.TextContent = "Clock: " + (Power.GetSystemClock() == SystemClock.High ? "480MHz" : "240MHz");
            this._demoVersionText.TextContent = "Demo version: " + DemoVersion;
        }
    }
}
