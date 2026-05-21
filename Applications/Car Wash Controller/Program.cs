using System;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace CarWashExample {
    internal class Program : Application {
        private const int ScreenWidth = 480;
        private const int ScreenHeight = 272;

        public Program(DisplayController d) : base(d) { }

        private static Program app;
        private static FT5xx6Controller touch;

        public static SelectServiceWindow SelectServicePage { get; private set; }
        public static PaymentWindow PaymentPage { get; private set; }
        public static LoadingPage LoadingPage { get; private set; }
        public static CarWashPage CarWashPage { get; private set; }
        public static EndPage EndPage { get; private set; }

        public static Window WpfWindow { get; private set; }

        /// <summary>Swap the active page and request a repaint. All page navigation
        /// goes through here so callers don't keep forgetting Invalidate.</summary>
        public static void NavigateTo(UIElement page) {
            WpfWindow.Child = page;
            WpfWindow.Invalidate();
        }

        static void Main() {
            WaitForStartButton();
            InitializeDisplayAndTouch();
            RunApp();
        }

        // The board comes up with the display blanked. Hold the LDR button on
        // power-on (or after reset) to start the demo; the on-board LED
        // blinks while we wait. Useful when debugging — gives time to attach
        // the debugger before the UI thread runs.
        private static void WaitForStartButton() {
            var gpio = GpioController.GetDefault();
            var led = gpio.OpenPin(SC20260.GpioPin.PB0);
            var ldr = gpio.OpenPin(SC20260.GpioPin.PB7);

            led.SetDriveMode(GpioPinDriveMode.Output);
            ldr.SetDriveMode(GpioPinDriveMode.InputPullUp);

            var seconds = 0;
            while (ldr.Read() == GpioPinValue.High) {
                led.Write(led.Read() == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);
                if (seconds % 10 == 0)
                    Debug.WriteLine("Waiting for LDR button: " + (seconds / 10) + "s");
                Thread.Sleep(100);
                seconds++;
            }

            led.Dispose();
            ldr.Dispose();
        }

        private static void InitializeDisplayAndTouch() {
            var gpio = GpioController.GetDefault();

            var backlight = gpio.OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var displayController = DisplayController.GetDefault();
            displayController.SetConfiguration(new ParallelDisplayControllerSettings {
                Width = ScreenWidth,
                Height = ScreenHeight,
                Orientation = DisplayOrientation.Degrees0,
                DataFormat = DisplayDataFormat.Rgb565,
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });
            displayController.Enable();

            var i2cDevice = I2cController.FromName(SC20260.I2cBus.I2c1).GetDevice(new I2cConnectionSettings(0x38) {
                BusSpeed = 100000,
                AddressFormat = I2cAddressFormat.SevenBit,
            });
            var interrupt = gpio.OpenPin(SC20260.GpioPin.PJ14);

            touch = new FT5xx6Controller(i2cDevice, interrupt) {
                Width = ScreenWidth,
                Height = ScreenHeight,
                Orientation = FT5xx6Controller.TouchOrientation.Degrees0,
            };
            touch.TouchDown += (s, e) => app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Down, DateTime.Now);
            touch.TouchUp   += (s, e) => app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Up,   DateTime.Now);

            app = new Program(displayController);
        }

        private static void RunApp() {
            SelectServicePage = new SelectServiceWindow();
            PaymentPage       = new PaymentWindow();
            LoadingPage       = new LoadingPage();
            CarWashPage       = new CarWashPage();
            EndPage           = new EndPage();

            WpfWindow = new Window {
                Width = ScreenWidth,
                Height = ScreenHeight,
                Background = new LinearGradientBrush(Colors.Blue, Colors.Teal, 0, 0, ScreenWidth, ScreenHeight),
                Child = SelectServicePage.Elements,
                Visibility = Visibility.Visible,
            };

            app.Run(WpfWindow);
        }
    }
}
