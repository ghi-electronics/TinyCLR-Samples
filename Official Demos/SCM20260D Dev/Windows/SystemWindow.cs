using System.Drawing;
using Demos.Properties;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class SystemWindow : ApplicationWindow {
        private Canvas canvas;

        private const string DemoVersion = "052126"; // May-21-2026 — last changed

        public SystemWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        private void CreateWindow() {
            const int startX = 20;
            const int offsetY = 30;
            var startY = 40;

            var font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.canvas.Children.Clear();

            var deviceText      = MakeLine(font, "Device: " + DeviceInformation.DeviceName);
            var clockText       = MakeLine(font, "Clock: " + (Power.GetSystemClock() == SystemClock.High ? "480MHz" : "240MHz"));
            var ramText         = MakeLine(font, "Memory: 512KB Total");
            var externalRamText = MakeLine(font, "External Memory: 32MB Total");
            var osText          = MakeLine(font, "OS: TinyCLR OS v3.0.0");
            var manufacturer    = MakeLine(font, "Manufacture: GHI Electronics, LLC");
            var demoVersionText = MakeLine(font, "Demo version: " + DemoVersion);

            var lines = new[] { deviceText, clockText, ramText, externalRamText, osText, manufacturer, demoVersionText };
            foreach (var line in lines) {
                Canvas.SetLeft(line, startX);
                Canvas.SetTop(line, startY);
                this.canvas.Children.Add(line);
                startY += offsetY;
            }

            Canvas.SetLeft(this.TopBar, 0);
            Canvas.SetTop(this.TopBar, 0);
            this.canvas.Children.Add(this.TopBar);
        }

        private static Text MakeLine(Font font, string text) => new Text(font, text) { ForeColor = Colors.White };

        protected override void Active() {
            this.canvas = new Canvas();
            this.CreateWindow();
            this.Child = this.canvas;
        }

        protected override void Deactive() => this.canvas.Children.Clear();
    }
}
