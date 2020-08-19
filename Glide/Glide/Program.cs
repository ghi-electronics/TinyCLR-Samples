using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Glide;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Glide.Properties;

namespace Glide
{
    class Program : Application
    {
        public static Program MainApp;

        public Program(DisplayController d) : base(d)
        {

        }

        static void Main()
        {
            Display.InitializeDisplay();
            Input.Touch.InitializeTouch();

            MainApp = new Program(Display.DisplayController);

            GHIElectronics.TinyCLR.Glide.Glide.SetupGlide(Display.Width, Display.Height, 96, 0, Display.DisplayController);            

            Window window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.GridDemo));
            GHIElectronics.TinyCLR.Glide.Glide.MainWindow = (GHIElectronics.TinyCLR.UI.Controls.GlideWindow)window;           

            MainApp.Run(GHIElectronics.TinyCLR.Glide.Glide.MainWindow); 
        }        
    }
}
