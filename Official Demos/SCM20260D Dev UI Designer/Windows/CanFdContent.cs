using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for CanFdContent.tcui. Exposes the two designed elements so the hosting CanFdWindow
    // (an ApplicationWindow) can drive them — the status TextFlow it fills with test output, and the Test
    // button it wires + hides while a test runs.
    public partial class CanFdContent : Canvas {
        public CanFdContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
    }
}
