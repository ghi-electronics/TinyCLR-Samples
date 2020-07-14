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
            var iconImageEthernet = Resources.GetBitmap(Resources.BitmapResources.Wifi); // Icon
            var iconTextEthernet = "Wifi";
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


            // Create I2C Window
            var iconImageI2c = Resources.GetBitmap(Resources.BitmapResources.I2c); // Icon
            var iconTextI2c = "I2C";
            var i2cWindow = new I2cWindow(iconImageI2c, iconTextI2c, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(i2cWindow); // Register to MainWindow

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
            var iconTextColor = "Color";
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
            var iconImageTemplate = Resources.GetBitmap(Resources.BitmapResources.Template); // Icon
            var iconTextTemplate = "Template"; // Text
            var templateWindow = new TemplateWindow(iconImageTemplate, iconTextTemplate, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableButtomNext = true,
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(templateWindow); // Register to MainWindow





            MainApp.Run(mainWindow);
        }
    }
}
