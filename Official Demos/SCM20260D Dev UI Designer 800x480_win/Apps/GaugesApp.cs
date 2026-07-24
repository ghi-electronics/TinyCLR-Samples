using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Gauge Styles app fragment. Layout is in Apps\GaugesApp.tcui; it shows three gauges
    // sharing MinValue/MaxValue but differing SweepAngle (180 / 270 / 360 degrees).
    // Root layout is a Grid (3 columns) — see GaugesApp.tcui.
    public partial class GaugesApp : Grid {
        public GaugesApp() => InitializeComponent();
    }
}
