using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.UI;

namespace Demos {
    class Program : Application {
        public static Program MainApp;

        public Program(DisplayController d) : base(d) {
        }

        static void Main() {
            Display.InitializeDisplay();
            Input.Touch.InitializeTouch();

            MainApp = new Program(Display.DisplayController);

            var mainWindow = new MainWindow(Display.Width, Display.Height);

            // System info.
            var systemWindow = new SystemWindow(
                Resources.GetBitmap(Resources.BitmapResources.settingImage),
                "System Information", Display.Width, Display.Height);
            mainWindow.RegisterWindow(systemWindow);

            // WiFi (Winc15x0 on MikroBus 1).
            var wifiWindow = new WifiWindow(
                Resources.GetBitmap(Resources.BitmapResources.Wifi),
                "Wifi", Display.Width, Display.Height);
            mainWindow.RegisterWindow(wifiWindow);

            // microSD storage.
            var sdWindow = new SdWindow(
                Resources.GetBitmap(Resources.BitmapResources.Sd),
                "Sd Card", Display.Width, Display.Height);
            mainWindow.RegisterWindow(sdWindow);

            // USB host mass-storage.
            var usbWindow = new UsbWindow(
                Resources.GetBitmap(Resources.BitmapResources.Usb),
                "Usb", Display.Width, Display.Height);
            mainWindow.RegisterWindow(usbWindow);

            // CAN-FD (CAN1).
            var canFdWindow = new CanFdWindow(
                Resources.GetBitmap(Resources.BitmapResources.Canfd),
                "CAN FD", Display.Width, Display.Height);
            mainWindow.RegisterWindow(canFdWindow);

            // Quad-SPI on-board flash.
            var qspiWindow = new QspiWindow(
                Resources.GetBitmap(Resources.BitmapResources.Qspi),
                "Quad Spi", Display.Width, Display.Height);
            mainWindow.RegisterWindow(qspiWindow);

            // Piezo buzzer (PWM).
            var buzzerWindow = new BuzzerWindow(
                Resources.GetBitmap(Resources.BitmapResources.Piezo),
                "Buzzer", Display.Width, Display.Height);
            mainWindow.RegisterWindow(buzzerWindow);

            // Real-time clock.
            var rtcWindow = new RtcWindow(
                Resources.GetBitmap(Resources.BitmapResources.Rtc),
                "Rtc", Display.Width, Display.Height);
            mainWindow.RegisterWindow(rtcWindow);

            // UART5 echo test.
            var uartWindow = new UartWindow(
                Resources.GetBitmap(Resources.BitmapResources.Uart),
                "Uart", Display.Width, Display.Height);
            mainWindow.RegisterWindow(uartWindow);

            // ADC.
            var adcWindow = new AdcWindow(
                Resources.GetBitmap(Resources.BitmapResources.analog),
                "Adc", Display.Width, Display.Height);
            mainWindow.RegisterWindow(adcWindow);

            // PWM.
            var pwmWindow = new PwmWindow(
                Resources.GetBitmap(Resources.BitmapResources.Pwm),
                "Pwm", Display.Width, Display.Height);
            mainWindow.RegisterWindow(pwmWindow);

            // DAC.
            var dacWindow = new DacWindow(
                Resources.GetBitmap(Resources.BitmapResources.analog),
                "Dac", Display.Width, Display.Height);
            mainWindow.RegisterWindow(dacWindow);

            // Camera (Omnivision OV9655).
            var cameraWindow = new CameraWindow(
                Resources.GetBitmap(Resources.BitmapResources.Camera),
                "Camera", Display.Width, Display.Height);
            mainWindow.RegisterWindow(cameraWindow);

            // Display colour test.
            var colorWindow = new ColorWindow(
                Resources.GetBitmap(Resources.BitmapResources.Color),
                "Color", Display.Width, Display.Height);
            mainWindow.RegisterWindow(colorWindow);

            // Basic LED / button / press test.
            var basicTestWindow = new BasicTestWindow(
                Resources.GetBitmap(Resources.BitmapResources.Generaltest),
                "Basic Test", Display.Width, Display.Height);
            mainWindow.RegisterWindow(basicTestWindow);

            // Empty template — copy this when adding a new app.
            var templateWindow = new TemplateWindow(
                Resources.GetBitmap(Resources.BitmapResources.Template),
                "Template", Display.Width, Display.Height) {
                EnableButtonNext = true,
                EnableButtonBack = true,
            };
            mainWindow.RegisterWindow(templateWindow);

            MainApp.Run(mainWindow);
        }
    }
}
