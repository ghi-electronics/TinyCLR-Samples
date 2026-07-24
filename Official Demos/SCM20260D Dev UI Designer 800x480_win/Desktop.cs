using System;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
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

            // The Start flyout starts hidden; the Start button toggles it (Windows 11).
            this._startMenu.Visibility = Visibility.Collapsed;

            // System-tray date (Windows 11 shows the date beneath the clock). Set once — fine for a demo.
            var today = DateTime.Now;
            this._dateText.TextContent = today.Month + "/" + today.Day + "/" + today.Year;

            // The clock runs continuously — including on the Camera screen. Its per-second update repaints only its
            // own ~66px rect in the top-right (fixed-width Text → Invalidate, not a layout pass), which never
            // overlaps the camera feed, so there's no flicker and the time stays live everywhere.
            this.clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            this.clock.Tick += this.OnTick;
            this.clock.Start();

            this.ShowApp(() => new AdcApp(), "Analog Input", 296); // default app window
        }

        private void ShowApp(AppFactory create, string title, int pillCenterX) {
            // Stop the outgoing app's live work FIRST, THEN build the next app. Building a heavy app (GaugesApp
            // decodes 3 dial images) must not race the still-running camera thread for the CPU — that's what made
            // switching from the Camera window to Gauge Styles feel unresponsive (needing several taps).
            if (this._content.Child is CameraApp cam) {
                cam.Stop();
            }
            else if (this._content.Child is AdcApp adc) {
                adc.Stop();
            }

            this._windowTitle.TextContent = title; // window chrome title (left, Windows 11 style)
            this._content.Child = create();
            this._content.Invalidate();

            // Slide the Windows 11 taskbar indicator pill under the active app's icon.
            Canvas.SetLeft(this._taskPill, pillCenterX - 9);
            this._taskPill.Invalidate();

            // Any app launch closes the Start flyout.
            this._startMenu.Visibility = Visibility.Collapsed;
        }

        private void OnTick(object sender, EventArgs e) {
            var now = DateTime.Now; // StatusText is now just the clock; battery %/Wi-Fi are static icons beside it
            this._statusText.TextContent = Pad2(now.Hour) + ":" + Pad2(now.Minute) + ":" + Pad2(now.Second);
        }

        private static string Pad2(int n) => (n < 10 ? "0" : "") + n;

        // Taskbar Start button — Windows 11 "Start": toggles the Start flyout (its tiles reuse the handlers below,
        // and any launch closes it again via ShowApp).
        private void OnStart(object sender, RoutedEventArgs e) {
            this._startMenu.Visibility = this._startMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            this._startMenu.Invalidate();
        }

        // Taskbar + Start-flyout icon handlers — swap the app in the window card, set its title, and slide the
        // taskbar pill under the matching icon (pillCenterX = that icon's centre in Desktop.tcui).
        private void OnDockAdc(object sender, RoutedEventArgs e) => this.ShowApp(() => new AdcApp(), "Analog Input", 296);
        private void OnDockGauges(object sender, RoutedEventArgs e) => this.ShowApp(() => new GaugesApp(), "Gauge Styles", 338);
        private void OnDockFiles(object sender, RoutedEventArgs e) => this.ShowApp(() => new FilesApp(), "Files", 380);
        private void OnDockTable(object sender, RoutedEventArgs e) => this.ShowApp(() => new TableApp(), "ADC Channels", 422);
        private void OnDockCalendar(object sender, RoutedEventArgs e) => this.ShowApp(() => new CalendarApp(), "Date & Time", 464);
        private void OnDockSettings(object sender, RoutedEventArgs e) => this.ShowApp(() => new SettingsApp(), "Settings", 506);
        private void OnDockCamera(object sender, RoutedEventArgs e) => this.ShowApp(() => new CameraApp(), "Camera", 548);
    }
}
