using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for DacContent.tcui. Exposes the two designed elements so the hosting DacWindow
    // (an ApplicationWindow) can drive them — the status TextFlow it fills with test output, and the Test
    // button it wires + hides while a test runs.
    public partial class DacContent : Canvas {
        public DacContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
    }
}
