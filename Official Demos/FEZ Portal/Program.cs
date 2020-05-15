using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace FEZ_Portal {
    class Program : Application {

        const int SCREEN_WIDTH = 480;
        const int SCREEN_HEIGHT = 272;
        const int TOUCH_IRQ = SC20260.GpioPin.PG9;
        const int BACKLIGHT = 0 * 16 + 15;
        const int LED1 = 7 * 16 + 6;
        const int LDR1 = 1 * 16 + 7;
        public Program(DisplayController d) : base(d) {
        }

        static GpioPin led1;
        static GpioPin ldr1;

        static void Main() {
            var controller = GpioController.GetDefault();
            led1 = controller.OpenPin(LED1);
            ldr1 = controller.OpenPin(LDR1);
            led1.SetDriveMode(GpioPinDriveMode.Output);
            ldr1.SetDriveMode(GpioPinDriveMode.InputPullUp);

            DisplayConfigWPF();
        }

        static Program app;
        static FT5xx6Controller touch;
        static void DisplayConfigWPF() {

            var backlight = GpioController.GetDefault().OpenPin(BACKLIGHT);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var displayController = GHIElectronics.TinyCLR.Devices.Display.DisplayController.GetDefault();
            var controllerSetting = new GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings {
                // 480x272
                Width = SCREEN_WIDTH,
                Height = SCREEN_HEIGHT,
                DataFormat = GHIElectronics.TinyCLR.Devices.Display.DisplayDataFormat.Rgb565,
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
            };

            displayController.SetConfiguration(controllerSetting);
            displayController.Enable();

            var i2cController = I2cController.FromName(SC20260.I2cBus.I2c1);

            var settings = new I2cConnectionSettings(0x38) {// the slave's address
                BusSpeed = 100000,
                AddressFormat = I2cAddressFormat.SevenBit,
            };

            var i2cDevice = i2cController.GetDevice(settings);

            var interrupt = GpioController.GetDefault().OpenPin(TOUCH_IRQ);

            touch = new FT5xx6Controller(i2cDevice, interrupt);
            touch.TouchDown += Touch_TouchDown;
            touch.TouchUp += Touch_TouchUp;

            // Create WPF window
            app = new Program(displayController);

            SelectPage = new SelectPage();
            ConnectPage = new ConnectPage("");
            SplashPage = new SplashPage();

            WpfWindow = Program.CreateWindow(displayController);
            WpfWindow.Child = SplashPage.Elements;
            WpfWindow.Visibility = Visibility.Visible;

            app.Run(WpfWindow);
        }
        public static SplashPage SplashPage { get; set; }
        public static SelectPage SelectPage { get; set; }
        public static ConnectPage ConnectPage { get; set; }
        public static Window WpfWindow { get; set; }
        private static void Touch_TouchUp(FT5xx6Controller sender, TouchEventArgs e) => app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Up, System.DateTime.Now);
        private static void Touch_TouchDown(FT5xx6Controller sender, TouchEventArgs e) => app.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Down, System.DateTime.Now);

        private static Window CreateWindow(DisplayController disp) {
            var window = new Window {
                Height = (int)disp.ActiveConfiguration.Height,
                Width = (int)disp.ActiveConfiguration.Width
            };
            window.Background = new LinearGradientBrush(Colors.Blue, Colors.Teal, 0, 0,
              window.Width, window.Height);

            return window;
        }
    }
}

