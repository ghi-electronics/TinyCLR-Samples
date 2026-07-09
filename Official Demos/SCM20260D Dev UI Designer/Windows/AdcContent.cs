using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for AdcContent.tcui. Exposes the two designed elements so the hosting AdcWindow
    // (an ApplicationWindow) can drive them — the status TextFlow it fills with the ADC readings, and the
    // Test button it wires + hides while a test runs.
    public partial class AdcContent : Canvas {
        public AdcContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
    }
}
