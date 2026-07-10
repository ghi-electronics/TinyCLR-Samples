using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    // Analog Input app fragment. Layout is in Apps\AdcApp.tcui. Fills the area chart with a fake trend, and
    // DRIFTS the gauge needle on a 100 ms timer so you can watch a real-time value update.
    //
    // No-flicker note: setting Gauge.Value calls Invalidate() (not MarkDirty), and the dial face is cached — so
    // each tick just re-blits the cached dial + redraws the needle in the GAUGE's own region. The framework
    // flushes that dirty rect, not the whole window, so the rest of the screen is untouched (no flicker). This is
    // the normal UI path — unlike the camera, which writes straight to the display and fights the UI buffer.
    public partial class AdcApp : Canvas {
        private readonly DispatcherTimer timer;
        private double phase;

        public AdcApp() {
            InitializeComponent();

            // Fake signal trend for the area chart.
            var items = new ArrayList();
            var vals = new double[] { 12, 18, 9, 22, 16, 28, 20, 25 };
            for (var i = 0; i < vals.Length; i++) {
                items.Add(new Chart.DataItem { Name = (i + 1).ToString(), Value = vals[i] });
            }
            this._trendChart.Items = items;
            this._trendChart.Refresh();

            // Fake live ADC: sweep the needle across the 0..3300 range so its motion is obviously smooth (any
            // flicker would stand out against it).
            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            this.timer.Tick += this.OnTick;
            this.timer.Start();
        }

        // Called by the shell before swapping this app out, so the timer stops firing.
        public void Stop() => this.timer.Stop();

        private void OnTick(object sender, EventArgs e) {
            this.phase += 0.12;
            this._adcGauge.Value = (float)(1650 + 1450 * Math.Sin(this.phase)); // oscillate around mid-scale
        }
    }
}
