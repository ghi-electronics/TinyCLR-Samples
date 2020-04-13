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
            var iconTextSystem = "System Information";
            var systemWindow = new SystemWindow(iconImageSystem, iconTextSystem, Display.Width, Display.Height) {
                EnableButtomBack = true,
                EnableClockOnTopBar = true
            };
            
            mainWindow.RegisterWindow(systemWindow);

            // Create Ethernet Window
            var iconImageEthernet = Resources.GetBitmap(Resources.BitmapResources.Ethernet); // Icon
            var iconTextEthernet = "Ethernet";
            var networkWindow = new EthernetWindow(iconImageEthernet, iconTextEthernet, Display.Width, Display.Height) {
                EnableButtomBack = true,
               
                EnableClockOnTopBar = true
            };

            mainWindow.RegisterWindow(networkWindow); // Register to MainWindow

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
