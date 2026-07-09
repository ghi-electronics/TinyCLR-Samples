using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for UartContent.tcui. Exposes the two designed elements so the hosting UartWindow
    // (an ApplicationWindow) can drive them — the status TextFlow it fills with test output, and the Test
    // button it wires + hides while a test runs.
    public partial class UartContent : Canvas {
        public UartContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
    }
}
