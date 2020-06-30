using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;

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

            // Create System Window            
            var iconImageSystem = Resources.GetBitmap(Resources.BitmapResources.settingImage);
            var iconTextSystem = "System Information";
            var systemWindow = new SystemWindow(iconImageSystem, iconTextSystem, Display.Width, Display.Height);

            mainWindow.RegisterWindow(systemWindow);

            // Create Ethernet Window
            var iconImageEthernet = Resources.GetBitmap(Resources.BitmapResources.Ethernet); // Icon
            var iconTextEthernet = "Ethernet";
            var networkWindow = new EthernetWindow(iconImageEthernet, iconTextEthernet, Display.Width, Display.Height);

            mainWindow.RegisterWindow(networkWindow); // Register to MainWindow

            // Create Wifi Window
            var iconImageWifi = Resources.GetBitmap(Resources.BitmapResources.Wifi); // Icon
            var iconTextWifi = "Wifi";
            var wifiWindow = new WifiWindow(iconImageWifi, iconTextWifi, Display.Width, Display.Height);

            mainWindow.RegisterWindow(wifiWindow); // Register to MainWindow

            // Create Sd Window
            var iconImageSd = Resources.GetBitmap(Resources.BitmapResources.Sd); // Icon
            var iconTextSd = "Sd";
            var sdWindow = new SdWindow(iconImageSd, iconTextSd, Display.Width, Display.Height);

            mainWindow.RegisterWindow(sdWindow); // Register to MainWindow

            // Create Sd Window
            var iconImageUsb = Resources.GetBitmap(Resources.BitmapResources.Usb); // Icon
            var iconTextUsb = "Usb";
            var usbdWindow = new UsbWindow(iconImageUsb, iconTextUsb, Display.Width, Display.Height);

            mainWindow.RegisterWindow(usbdWindow); // Register to MainWindow

            // Empty template
            var iconImageTemplate = Resources.GetBitmap(Resources.BitmapResources.Template); // Icon
            var iconTextTemplate = "Template"; // Text
            var templateWindow = new TemplateWindow(iconImageTemplate, iconTextTemplate, Display.Width, Display.Height) {
                EnableButtonNext = true,
                EnableButtonBack = true,
            };

            mainWindow.RegisterWindow(templateWindow); // Register to MainWindow

            MainApp.Run(mainWindow);
        }
    }
}
