using System.Drawing;
using Demos.Properties;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class SystemWindow : ApplicationWindow {
        private Canvas canvas;

        const string DemoVersion = "071520"; // Jul-15-2020

        public SystemWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {

        }

        private void CreateWindow() {
            var startX = 20;
            var startY = 40;
            var offsetY = 30;
            var font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.canvas.Children.Clear();

            var devText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Device: " + GHIElectronics.TinyCLR.Native.DeviceInformation.DeviceName) {
                ForeColor = Colors.White,
            };

            var speed = Power.GetSystemClock() == SystemClock.High ? "Clock: 480MHz" : "Clock: 240MHz";

            var clockText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, speed) {
                ForeColor = Colors.White,
            };            

            var ramText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Memory: 512KB Total") {
                ForeColor = Colors.White,
            };

            var ramExtText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "External Memory: 32MB Total") {
                ForeColor = Colors.White,
            };

            var osText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "OS: TinyCLR OS v2.0.0") {
                ForeColor = Colors.White,
            };
            var mText = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Manufacture: GHI Electronics, LLC") {
                ForeColor = Colors.White,
            };

            var mVesion = new GHIElectronics.TinyCLR.UI.Controls.Text(font, "Demo version: " + DemoVersion) {
                ForeColor = Colors.White,
            };

            Canvas.SetLeft(devText, startX); Canvas.SetTop(devText, startY); startY += offsetY;
            Canvas.SetLeft(clockText, startX); Canvas.SetTop(clockText, startY); startY += offsetY;
            Canvas.SetLeft(ramText, startX); Canvas.SetTop(ramText, startY); startY += offsetY;
            Canvas.SetLeft(ramExtText, startX); Canvas.SetTop(ramExtText, startY); startY += offsetY;
            Canvas.SetLeft(osText, startX); Canvas.SetTop(osText, startY); startY += offsetY;
            Canvas.SetLeft(mText, startX); Canvas.SetTop(mText, startY); startY += offsetY;
            Canvas.SetLeft(mVesion, startX); Canvas.SetTop(mVesion, startY); startY += offsetY;

            this.canvas.Children.Add(devText);
            this.canvas.Children.Add(clockText);
            this.canvas.Children.Add(ramText);
            this.canvas.Children.Add(ramExtText);
            this.canvas.Children.Add(osText);
            this.canvas.Children.Add(mText);
            this.canvas.Children.Add(mVesion);

            // Enable TopBar
            Canvas.SetLeft(this.TopBar, 0); Canvas.SetTop(this.TopBar, 0);
            this.canvas.Children.Add(this.TopBar);
        }

        protected override void Active() {

            this.canvas = new Canvas();

            this.CreateWindow();

            this.Child = this.canvas;
        }

        protected override void Deactive() => this.canvas.Children.Clear();
    }
}
