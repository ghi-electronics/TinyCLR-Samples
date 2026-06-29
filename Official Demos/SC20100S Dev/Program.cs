using Demos.Properties;
using GHIElectronics.TinyCLR.UI;

namespace Demos {
    class Program : Application {
        public static Program MainApp;

        public Program(int width, int height) : base(width, height) {
        }

        static void Main() {
            Display.InitializeDisplay();
            Input.Button.InitializeButtons();

            MainApp = new Program(Display.Width, Display.Height);

            var mainWindow = new MainWindow(Display.Width, Display.Height);

            // Icons are passed by enum (Resources.BitmapResources) rather than
            // a pre-loaded Bitmap, so the actual pixel data is lazy-loaded by
            // Icon.EnsureLoaded only when the carousel is about to draw it.
            // Eagerly decoding all icons at startup was tipping the SC20100
            // over its RAM limit.

            // System info — Back / clock on top bar.
            var systemWindow = new SystemWindow(
                Resources.BitmapResources.settingImage,
                "System", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(systemWindow);

            // Ethernet / Wi-Fi info.
            var networkWindow = new NetworkWindow(
                Resources.BitmapResources.Ethernet,
                "Eth-Wifi", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(networkWindow);

            // Piezo buzzer (PWM).
            var buzzerWindow = new BuzzerWindow(
                Resources.BitmapResources.Piezo,
                "Buzzer", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(buzzerWindow);

            // microSD storage.
            var sdWindow = new SdWindow(
                Resources.BitmapResources.Sd,
                "Sd Card", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(sdWindow);

            // USB host mass-storage.
            var usbWindow = new UsbWindow(
                Resources.BitmapResources.Usb,
                "Usb", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(usbWindow);

            // Quad-SPI on-board flash.
            var qspiWindow = new QspiWindow(
                Resources.BitmapResources.Qspi,
                "Quad Spi", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(qspiWindow);

            // CAN-FD (CAN1).
            var canFdWindow = new CanFdWindow(
                Resources.BitmapResources.Canfd,
                "CAN FD", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(canFdWindow);

            // UART1 echo test.
            var uartWindow = new UartWindow(
                Resources.BitmapResources.Uart,
                "Uart", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(uartWindow);

            // Real-time clock.
            var rtcWindow = new RtcWindow(
                Resources.BitmapResources.Rtc,
                "Rtc", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(rtcWindow);

            // PWM (red + green leds).
            var pwmWindow = new PwmWindow(
                Resources.BitmapResources.Pwm,
                "Led - Pwm", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(pwmWindow);

            // Display colour test.
            var colorWindow = new ColorWindow(
                Resources.BitmapResources.Color,
                "Test Color", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(colorWindow);

            // Basic LED / press / production test.
            var basicTestWindow = new BasicTestWindow(
                Resources.BitmapResources.Basictest,
                "Basic Test", Display.Width, Display.Height) {
                EnableButtonBack = true,
                EnableButtonNext = true,
                EnableClockOnTopBar = true,
            };
            mainWindow.RegisterWindow(basicTestWindow);

            MainApp.Run(mainWindow);
        }
    }
}
