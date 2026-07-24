using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    // TinyCLR_OS — a macOS-style desktop shell that showcases the UI control set on an 800x480 (7") panel.
    internal class Program : Application {
        internal const int ScreenWidth = 800;
        internal const int ScreenHeight = 480;

        // ==== Screen-capture flag (diagnostic aid) ==================================================
        // OFF by default: the app never touches the SD card and no card is required to run.
        // Flip to true to save the current screen to the SD card as a .bmp each time PB7 is pressed
        // (screen0.bmp, screen1.bmp, ...) — useful for sharing exactly what's on the device.
        private static readonly bool EnableScreenCapture = true;
        // ===========================================================================================

        public static Program App;
        private static FT5xx6Controller touch;   // held so its interrupt isn't GC'd

        private static Window shellWindow;             // the running Desktop (invalidated to force a flush on capture)
        private static GpioPin captureButton;          // held so its interrupt isn't GC'd
        private static string sdRoot;                  // SD mount path; null if no card / capture disabled
        private static volatile bool captureRequested;
        private static int screenshotIndex;

        public Program(DisplayController d) : base(d) { }

        private static void Main() {
            // An 800x480 UI plus the chart/gauge/image bitmaps needs the external RAM. Extend the heap on
            // first boot (then reset so the extended heap is in effect). Without this the app hard-faults / OOMs.
            if (!Memory.IsExtendedHeap()) {
                Memory.ExtendHeap();
                Power.Reset();
            }

            var display = InitializeDisplay();
            App = new Program(display);
            InitializeTouch();

            shellWindow = new Desktop();

            if (EnableScreenCapture) {
                InitializeScreenCapture();
            }

            App.Run(shellWindow);
        }

        // 800x480 parallel-RGB panel (timings from the RotarodRepro board).
        private static DisplayController InitializeDisplay() {
            var backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var display = DisplayController.GetDefault();
            display.SetConfiguration(new ParallelDisplayControllerSettings {
                Width = ScreenWidth,
                Height = ScreenHeight,
                DataFormat = DisplayDataFormat.Rgb565,
                Orientation = DisplayOrientation.Degrees0,
                PixelClockRate = 40000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 200,
                HorizontalBackPorch = 1,
                HorizontalSyncPulseWidth = 87,
                HorizontalSyncPolarity = true,
                VerticalFrontPorch = 101,
                VerticalBackPorch = 29,
                VerticalSyncPulseWidth = 3,
                VerticalSyncPolarity = true,
            });
            display.Enable();
            return display;
        }

        // FT5xx6 capacitive touch on I2C1 @0x38, interrupt PJ14.
        private static void InitializeTouch() {
            var i2c = I2cController.FromName(SC20260.I2cBus.I2c1).GetDevice(new I2cConnectionSettings(0x38) {
                BusSpeed = 100000,
                AddressFormat = I2cAddressFormat.SevenBit,
            });
            var interrupt = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PJ14);

            touch = new FT5xx6Controller(i2c, interrupt) {
                Width = ScreenWidth,
                Height = ScreenHeight,
                Orientation = FT5xx6Controller.TouchOrientation.Degrees0,
            };
            touch.TouchDown += (s, e) => App.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Down, DateTime.Now);
            touch.TouchUp += (s, e) => App.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Up, DateTime.Now);
        }

        // Only wired when EnableScreenCapture is true. Mounts the SD card and wires PB7 to save a screenshot.
        private static void InitializeScreenCapture() {
            try {
                var sd = StorageController.FromName(SC20260.StorageController.SdCard);
                var drive = FileSystem.Mount(sd.Hdc);
                sdRoot = drive.Name; // e.g. "A:\"
                Debug.WriteLine("Screen capture ready: SD mounted at " + sdRoot + ". Press PB7 to capture the screen.");
            }
            catch (Exception ex) {
                sdRoot = null;
                Debug.WriteLine("Screen capture: SD mount failed (" + ex.Message + "). Insert an SD card to enable PB7 capture.");
            }

            Graphics.OnFlushEvent += OnScreenFlush;

            captureButton = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PB7);
            captureButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            captureButton.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            captureButton.ValueChanged += OnCaptureButtonChanged;
        }

        private static void OnCaptureButtonChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge != GpioPinEdge.FallingEdge || sdRoot == null || captureRequested)
                return;

            captureRequested = true;

            // Force a redraw so the display flushes (a static screen wouldn't flush on its own); Invalidate has
            // UI-thread affinity, so marshal it onto the dispatcher.
            App.Dispatcher.BeginInvoke(new DispatcherOperationCallback(o => { shellWindow?.Invalidate(); return null; }), null);
        }

        // Fires on every flush; acts only when a capture was requested. 'data' is the FULL framebuffer in the
        // display's 16bpp (RGB565) format; 'originalWidth' is the full surface width.
        private static void OnScreenFlush(Graphics sender, byte[] data, int x, int y, int width, int height, int originalWidth) {
            if (!captureRequested)
                return;

            captureRequested = false;

            var w = originalWidth;
            var h = data.Length / (w * 2); // 16bpp => 2 bytes/pixel
            var snapshot = (byte[])data.Clone();

            new Thread(() => SaveScreenshot(snapshot, w, h)).Start();
        }

        private static void SaveScreenshot(byte[] framebuffer, int width, int height) {
            var root = sdRoot;
            if (root == null)
                return;

            var path = root + "screen" + screenshotIndex + ".bmp";
            screenshotIndex++;

            try {
                using (var bmp = new Bitmap(framebuffer, width, height))
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                    bmp.Save(stream, ImageFormat.Bmp);
                    stream.Flush();
                }

                Debug.WriteLine("Screen captured to " + path + " (" + width + "x" + height + ").");
            }
            catch (Exception ex) {
                Debug.WriteLine("Screen capture failed: " + ex.Message);
            }
        }
    }
}
