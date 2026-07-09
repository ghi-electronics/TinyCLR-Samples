using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for BasicTestContent.tcui. Exposes the three designed elements so the hosting
    // BasicTestWindow (an ApplicationWindow) can drive them — the status TextFlow it fills with test
    // output, the Test button it hides while a test runs, and the Next button it shows/hides to advance
    // the individual test steps.
    public partial class BasicTestContent : Canvas {
        public BasicTestContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
        public Button NextButton => this._nextButton;
    }
}
