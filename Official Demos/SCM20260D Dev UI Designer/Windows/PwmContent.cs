using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for PwmContent.tcui. Exposes the two designed elements so the hosting PwmWindow
    // (an ApplicationWindow) can drive them — the status TextFlow it fills with test output, and the Test
    // button it wires + hides while a test runs.
    public partial class PwmContent : Canvas {
        public PwmContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
    }
}
