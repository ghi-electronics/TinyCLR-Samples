using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
//using GHIElectronics.TinyCLR.Devices.I2c;
//using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;

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

            // Create System Window            
            var iconImageSystem = Resources.GetBitmap(Resources.BitmapResources.settingImage);
            var iconTextSystem = "System";
            var systemWindow = new SystemWindow(iconImageSystem, iconTextSystem, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(systemWindow);

            // Create Ethernet Window
            var iconImageEthernet = Resources.GetBitmap(Resources.BitmapResources.Ethernet); // Icon
            var iconTextEthernet = "Eth-Wifi";
            var networkWindow = new NetworkWindow(iconImageEthernet, iconTextEthernet, Display.Width, Display.Height) {
                EnableButtomBack = true,

                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(networkWindow); // Register to MainWindow

            // Create Buzzer Window
            var iconImageBuzzer = Resources.GetBitmap(Resources.BitmapResources.Piezo); // Icon
            var iconTextBuzzer = "Buzzer";
            var buzzerWindow = new BuzzerWindow(iconImageBuzzer, iconTextBuzzer, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(buzzerWindow); // Register to MainWindow

            // Create Sd Window
            var iconImageSd = Resources.GetBitmap(Resources.BitmapResources.Sd); // Icon
            var iconTextSd = "Sd Card";
            var sdWindow = new SdWindow(iconImageSd, iconTextSd, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(sdWindow); // Register to MainWindow

            // Create Usb Window
            var iconImageUsb = Resources.GetBitmap(Resources.BitmapResources.Usb); // Icon
            var iconTextUsb = "Usb";
            var usbdWindow = new UsbWindow(iconImageUsb, iconTextUsb, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(usbdWindow); // Register to MainWindow

            // Create QSpi Window
            var iconImageQspi = Resources.GetBitmap(Resources.BitmapResources.Qspi); // Icon
            var iconTextQspi = "Quad Spi";
            var qspiWindow = new QspiWindow(iconImageQspi, iconTextQspi, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(qspiWindow); // Register to MainWindow

            // Create CanFd Window
            var iconImageCanFd = Resources.GetBitmap(Resources.BitmapResources.Canfd); // Icon
            var iconTextCanFd = "CAN FD";
            var canFdWindow = new CanFdWindow(iconImageCanFd, iconTextCanFd, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(canFdWindow); // Register to MainWindow

            // Create Uart Window
            var iconImageUart = Resources.GetBitmap(Resources.BitmapResources.Uart); // Icon
            var iconTextUart = "Uart";
            var uartWindow = new UartWindow(iconImageUart, iconTextUart, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(uartWindow); // Register to MainWindow

            // Create Buzzer Window
            var iconImageRtc = Resources.GetBitmap(Resources.BitmapResources.Rtc); // Icon
            var iconTextRtc = "Rtc";
            var rtcWindow = new RtcWindow(iconImageRtc, iconTextRtc, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(rtcWindow); // Register to MainWindow

            // Create Pwm Window
            var iconImagePwm = Resources.GetBitmap(Resources.BitmapResources.Pwm); // Icon
            var iconTextPwm = "Led - Pwm";
            var pwmWindow = new PwmWindow(iconImagePwm, iconTextPwm, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(pwmWindow); // Register to MainWindow

            // Create Camera Window
            var iconImageColor = Resources.GetBitmap(Resources.BitmapResources.Color); // Icon
            var iconTextColor = "Test Color";
            var colorWindow = new ColorWindow(iconImageColor, iconTextColor, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(colorWindow); // Register to MainWindow

            // Create Camera Window
            var iconImageBasicTest = Resources.GetBitmap(Resources.BitmapResources.Basictest); // Icon
            var iconTextBasicTest = "Basic Test";
            var basicTestWindow = new BasicTestWindow(iconImageBasicTest, iconTextBasicTest, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(basicTestWindow); // Register to MainWindow

            // Empty template
            //var iconImageTemplate = Resources.GetBitmap(Resources.BitmapResources.Template); // Icon
            //var iconTextTemplate = "Template"; // Text
            //var templateWindow = new TemplateWindow(iconImageTemplate, iconTextTemplate, Display.Width, Display.Height) {
            //    EnableButtomBack = true,
            //    EnableButtomNext = true,
            //    EnableClockOnTopBar = true
            //};

            //mainWindow.RegisterWindow(templateWindow); // Register to MainWindow





            MainApp.Run(mainWindow);
        }
    }
}
