using System;
using System.Drawing;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class SystemWindow : ApplicationWindow {
        private Canvas canvas;        

        public SystemWindow(Bitmap icon, string title, int width, int height) : base(icon, title, width, height) {

        }

        private void CreateWindow() {
            var startX = 10;
            var startY = 20;
            var offsetY = 10;
            var font = Resources.GetFont(Resources.FontResources.droid_reg08);

            this.canvas.Children.Clear();

            var devText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Device: " + GHIElectronics.TinyCLR.Native.DeviceInformation.DeviceName) {
                ForeColor = Colors.White,
            };

            var clockText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Clock: 480MHz") {
                ForeColor = Colors.White,
            };

            var osText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "OS: TinyCLR OS v2.0.0") {
                ForeColor = Colors.White,
            };
            var mText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Manufacture: GHI Electronics, LLC") {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(devText, startX); Canvas.SetTop(devText, startY); startY += offsetY;
            Canvas.SetLeft(clockText, startX); Canvas.SetTop(clockText, startY); startY += offsetY;
            Canvas.SetLeft(osText, startX); Canvas.SetTop(osText, startY); startY += offsetY;
            Canvas.SetLeft(mText, startX); Canvas.SetTop(mText, startY); startY += offsetY;

            this.canvas.Children.Add(devText);
            this.canvas.Children.Add(clockText);
            this.canvas.Children.Add(osText);
            this.canvas.Children.Add(mText);

            // Enable TopBar
            Canvas.SetLeft(this.TopBar, 0); Canvas.SetTop(this.TopBar, 0);
            this.canvas.Children.Add(this.TopBar);

            // Enable BottomBar
            Canvas.SetLeft(this.BottomBar, 2); Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
            this.canvas.Children.Add(this.BottomBar);

            this.OnBottomBarButtonUpEvent += this.SystemWindow_OnBottomBarButtonUpEvent;
        }

        private void SystemWindow_OnBottomBarButtonUpEvent(object sender, RoutedEventArgs e) => this.Close();

        protected override void Active() {
            
            this.canvas = new Canvas();

            this.CreateWindow();

            this.Child = this.canvas;
        }

        protected override void Deactive() => this.canvas.Children.Clear();

    }
}
