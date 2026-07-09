using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for CameraContent.tcui. Exposes the two designed elements so the hosting CameraWindow
    // (an ApplicationWindow) can drive them — the status TextFlow it fills with test output, and the Test
    // button it wires + hides while a test runs. The live camera frames are drawn directly to the display
    // by CameraWindow (raw DrawBuffer), not by any control in this fragment.
    public partial class CameraContent : Canvas {
        public CameraContent() => InitializeComponent();

        public TextFlow StatusText => this._statusText;
        public Button TestButton => this._testButton;
    }
}
