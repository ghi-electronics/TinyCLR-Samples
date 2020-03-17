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
        const int SCREEN_WIDTH = 480;
        const int SCREEN_HEIGHT = 272;

        public static Program MainApp;

        public Program(DisplayController d) : base(d) {

        }

        static void Main() {
            
            Display.InitializeDisplay();
            Touch.InitializeTouch();

            MainApp = new Program(Display.DisplayController);

            var mainWindow = new MainWindow(SCREEN_WIDTH, SCREEN_HEIGHT);

            // Create System Window            
            var iconImageSystem = Resources.GetBitmap(Resources.BitmapResources.settingImage);                  
            var iconTextSystem = "System Information";            
            var systemWindow = new SystemWindow(iconImageSystem, iconTextSystem, SCREEN_WIDTH, SCREEN_HEIGHT);            
            mainWindow.RegisterWindow(systemWindow);

            // Create Ethernet Window
            var iconImageEthernet = Resources.GetBitmap(Resources.BitmapResources.Ethernet); // Icon
            var iconTextEthernet = "Ethernet"; 
            var networkWindow = new EthernetWindow(iconImageEthernet, iconTextEthernet, SCREEN_WIDTH, SCREEN_HEIGHT); // Create                     
            mainWindow.RegisterWindow(networkWindow); // Register to MainWindow

            // Empty template
            var iconImageTemplate = Resources.GetBitmap(Resources.BitmapResources.Template); // Icon
            var iconTextTemplate = "Template"; // Text
            var templateWindow = new TemplateWindow(iconImageTemplate, iconTextTemplate, SCREEN_WIDTH, SCREEN_HEIGHT); // Create                     
            mainWindow.RegisterWindow(templateWindow); // Register to MainWindow

            MainApp.Run(mainWindow);
        }
    }
}
