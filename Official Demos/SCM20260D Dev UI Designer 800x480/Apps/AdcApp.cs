using System;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    // Analog Input app fragment. Layout AND the chart's INITIAL trend are in Apps\AdcApp.tcui (the <Point> series),
    // so the designer preview shows the same chart the device starts with. This code DRIFTS the gauge needle on a
    // 100 ms timer, and once per second refreshes the chart with new random values (in the same 0..30 range) so you
    // can watch it update live — the one thing a static .tcui can't show.
    //
    // No-flicker note: setting Gauge.Value calls Invalidate() (not MarkDirty), and the dial face is cached — so
    // each tick just re-blits the cached dial + redraws the needle in the GAUGE's own region. The framework
    // flushes that dirty rect, not the whole window, so the rest of the screen is untouched (no flicker). This is
    // the normal UI path — unlike the camera, which writes straight to the display and fights the UI buffer.
    public partial class AdcApp : Canvas {
        private readonly DispatcherTimer timer;
        private readonly Random random = new Random();
        private double phase;
        private int chartTicks;

        public AdcApp() {
            InitializeComponent(); // builds the layout AND the initial chart series from AdcApp.tcui

            // Fake live ADC: sweep the needle across the 0..3300 range so its motion is obviously smooth (any
            // flicker would stand out against it). The same timer also drives the once-per-second chart refresh.
            this.timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            this.timer.Tick += this.OnTick;
            this.timer.Start();
        }

        // Called by the shell before swapping this app out, so the timer stops firing.
        public void Stop() => this.timer.Stop();

        private void OnTick(object sender, EventArgs e) {
            this.phase += 0.12;
            this._adcGauge.Value = (float)(1650 + 1450 * Math.Sin(this.phase)); // oscillate around mid-scale

            // Once per second (every 10th 100 ms tick): give the chart new random values in the SAME 0..30 range.
            // The DataItems are reused (only their Value changes), so there is no per-second allocation.
            if (++this.chartTicks >= 10) {
                this.chartTicks = 0;
                foreach (Chart.DataItem item in this._trendChart.Items) {
                    item.Value = this.random.Next(31); // 0..30 inclusive
                }

                this._trendChart.Refresh(); // rebuild the cached chart bitmap + repaint the chart's region only
            }
        }
    }
}
