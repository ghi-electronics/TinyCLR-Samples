using System.Drawing;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class SystemWindow : ApplicationWindow {
        private Canvas canvas;

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
        }

        protected override void Active() {

            this.canvas = new Canvas();

            this.CreateWindow();

            this.Child = this.canvas;
        }

        protected override void Deactive() => this.canvas.Children.Clear();
    }
}
