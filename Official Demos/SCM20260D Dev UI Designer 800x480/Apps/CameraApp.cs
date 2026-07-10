using System;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.Omnivision.Ov9655;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    // Live camera test (OV9655 on the DCMI camera interface, sensor configured over I2C1). A worker thread
    // captures VGA frames and blits each one straight to the display (DisplayController.DrawBuffer) — the fast
    // path, so the measured rate reflects real capture+draw speed.
    //
    // IMPORTANT (why this doesn't flicker): the frames are written DIRECTLY to the display's front buffer, but the
    // UI renders to an OFF-SCREEN buffer and flushes the whole thing on ANY Invalidate — which would repaint the
    // white content card over the feed. So while the camera is live we (a) let the shell pause its 1s clock, and
    // (b) draw the fps as a DIRECT overlay (DrawBuffer of a small bitmap) instead of updating a UI Text. No UI
    // repaint happens over the feed → no white flicker (same approach as the SCM camera window).
    public partial class CameraApp : Canvas {
        // Camera feed region in SCREEN coordinates (inside the Content card), centre-cropped from the VGA frame.
        private const int FeedX = 130, FeedY = 92, FeedW = 540, FeedH = 292;
        private const int VgaWidth = 640, VgaHeight = 480;

        // fps/status overlay: a strip ABOVE the feed (frames never cover it) but BELOW the shell's window-chrome
        // title band, drawn straight to the display.
        private const int OverlayX = 132, OverlayY = 76, OverlayW = 320, OverlayH = 14;

        private volatile bool running;
        private volatile int frameCount;
        private volatile int currentFps = -1;
        private readonly DispatcherTimer fpsTimer;

        private System.Drawing.Font overlayFont;

        public CameraApp() {
            InitializeComponent();

            this.running = true;
            new Thread(this.CameraLoop).Start();

            // Only COMPUTES the fps (no UI work — updating a UI element would flush the card over the feed).
            this.fpsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            this.fpsTimer.Tick += this.OnFpsTick;
            this.fpsTimer.Start();
        }

        // Called by the shell before swapping this app out, so the feed thread stops writing over the display.
        public void Stop() {
            this.running = false;
            this.fpsTimer.Stop();
        }

        private void OnFpsTick(object sender, EventArgs e) {
            this.currentFps = this.frameCount;
            this.frameCount = 0;
        }

        private void CameraLoop() {
            // Run BELOW the UI thread so it always preempts us for touch + timers. Without this the tight
            // capture/DrawBuffer loop starves the dispatcher: touch stops responding and timers freeze until the
            // camera happens to yield (the "touch does nothing for 15-20s" symptom).
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            var display = DisplayController.GetDefault();
            var cam = this.TryInitCamera();

            if (cam == null) {
                this.DrawOverlay(display, "Camera not found — connect the OV9655 module");
                return;
            }

            var srcX = (VgaWidth - FeedW) / 2;
            var srcY = (VgaHeight - FeedH) / 2;
            var shownFps = -2;

            this.DrawOverlay(display, "Live feed…");

            while (this.running) {
                try {
                    if (cam.Capture()) {
                        display.DrawBuffer(FeedX, FeedY, srcX, srcY, FeedW, FeedH, VgaWidth, cam.Buffer, 0);
                        this.frameCount++;
                    }
                }
                catch {
                }

                // Yield a slice each frame so the UI dispatcher (touch + the fps timer) gets the CPU and the
                // display isn't held back-to-back. Small enough not to meaningfully cap the frame rate.
                Thread.Sleep(5);

                // Refresh the overlay only when the measured rate changes (~once/second) — drawn directly, so it
                // never triggers a UI repaint of the card.
                var fps = this.currentFps;
                if (fps != shownFps) {
                    this.DrawOverlay(display, "Live — " + fps + " fps");
                    shownFps = fps;
                }
            }
        }

        private Ov9655Controller TryInitCamera() {
            try {
                var i2c = I2cController.FromName(SC20260.I2cBus.I2c1);

                // Some modules fail to initialise on the first reset; retry once (mirrors the SCM demo).
                for (var i = 0; i < 2; i++) {
                    try {
                        var cam = new Ov9655Controller(i2c);
                        cam.SetResolution(Ov9655Controller.Resolution.Vga);
                        return cam;
                    }
                    catch {
                    }
                }
            }
            catch {
            }

            return null;
        }

        // Renders the status text into a small RGB565 bitmap and blits it straight to the display (no UI). Reuses
        // one buffer; called at most ~once per second, so it costs nothing per frame.
        private void DrawOverlay(DisplayController display, string text) {
            if (this.overlayFont == null)
                this.overlayFont = Resources.GetFont(Resources.FontResources.droid_reg12);

            using (var bmp = new System.Drawing.Bitmap(OverlayW, OverlayH))
            using (var g = System.Drawing.Graphics.FromImage(bmp)) {
                using (var bg = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0xFF, 0xFB, 0xFB, 0xFD))) // card colour
                    g.FillRectangle(bg, 0, 0, OverlayW, OverlayH);
                using (var fg = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0xFF, 0x25, 0x63, 0xEB))) // blue
                    g.DrawString(text, this.overlayFont, fg, 0, 0);

                // Blit while the bitmap is alive (GetBitmap can hand back the live surface buffer).
                display.DrawBuffer(OverlayX, OverlayY, 0, 0, OverlayW, OverlayH, OverlayW, bmp.GetBitmap(), 0);
            }
        }
    }
}
