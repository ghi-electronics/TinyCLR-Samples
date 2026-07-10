using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Settings app fragment. Layout is in Apps\SettingsApp.tcui; this just wires up the
    // generated control tree. The showcased controls (CheckBox, RadioButton, Slider, ComboBox,
    // ProgressBar) render with their designer values — no runtime data feed needed.
    // Root layout is a vertical StackPanel with nested horizontal rows — see SettingsApp.tcui.
    public partial class SettingsApp : StackPanel {
        public SettingsApp() => InitializeComponent();
    }
}
