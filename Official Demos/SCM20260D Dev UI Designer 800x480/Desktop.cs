using System;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    // The desktop shell. Layout is in Desktop.tcui; this shows the default app, swaps apps when a Dock icon
    // is tapped, and updates the status clock on a 1-second timer (nothing runs per frame).
    public partial class Desktop : Window {
        private readonly DispatcherTimer clock;

        // Builds an app on demand. Deferring creation lets ShowApp stop the OUTGOING app first (see ShowApp).
        private delegate UIElement AppFactory();

        public Desktop() {
            InitializeComponent();

            // The clock runs continuously — including on the Camera screen. Its per-second update repaints only its
            // own ~66px rect in the top-right (fixed-width Text → Invalidate, not a layout pass), which never
            // overlaps the camera feed, so there's no flicker and the time stays live everywhere.
            this.clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            this.clock.Tick += this.OnTick;
            this.clock.Start();

            // The default app (AdcApp / "Analog Input") is declared in Desktop.tcui as <local:AdcApp/> inside
            // Content, so the UI Designer previews it. InitializeComponent already created it and started its timer;
            // the Dock swaps it for another app via ShowApp() below.
        }

        private void ShowApp(AppFactory create, string title) {
            // Stop the outgoing app's live work FIRST, THEN build the next app. Building a heavy app (GaugesApp
            // decodes 3 dial images) must not race the still-running camera thread for the CPU — that's what made
            // switching from the Camera window to Gauge Styles feel unresponsive (needing several taps).
            if (this._content.Child is CameraApp cam) {
                cam.Stop();
            }
            else if (this._content.Child is AdcApp adc) {
                adc.Stop();
            }

            this._windowTitle.TextContent = title; // window chrome title (centred, next to the traffic-light dots)
            this._content.Child = create();
            this._content.Invalidate();
        }

        private void OnTick(object sender, EventArgs e) {
            var now = DateTime.Now; // StatusText is now just the clock; battery %/Wi-Fi are static icons beside it
            this._statusText.TextContent = Pad2(now.Hour) + ":" + Pad2(now.Minute) + ":" + Pad2(now.Second);
        }

        private static string Pad2(int n) => (n < 10 ? "0" : "") + n;

        // Dock icon handlers — swap the app shown in the window card (and set its window-chrome title).
        private void OnDockAdc(object sender, RoutedEventArgs e) => this.ShowApp(() => new AdcApp(), "Analog Input");
        private void OnDockGauges(object sender, RoutedEventArgs e) => this.ShowApp(() => new GaugesApp(), "Gauge Styles");
        private void OnDockFiles(object sender, RoutedEventArgs e) => this.ShowApp(() => new FilesApp(), "Files");
        private void OnDockTable(object sender, RoutedEventArgs e) => this.ShowApp(() => new TableApp(), "ADC Channels");
        private void OnDockCalendar(object sender, RoutedEventArgs e) => this.ShowApp(() => new CalendarApp(), "Date & Time");
        private void OnDockSettings(object sender, RoutedEventArgs e) => this.ShowApp(() => new SettingsApp(), "Settings");
        private void OnDockCamera(object sender, RoutedEventArgs e) => this.ShowApp(() => new CameraApp(), "Camera");
    }
}
