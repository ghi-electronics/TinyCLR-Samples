using System;
using CarWashExample.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;
using SystemDrawing = System.Drawing;

namespace CarWashExample {
    internal sealed class CarWashPage {
        private readonly Canvas canvas = new Canvas();
        private readonly SystemDrawing.Font font = Resources.GetFont(Resources.FontResources.NinaB);
        private readonly ProgressBar progressBar;
        private readonly DispatcherTimer timer;

        public UIElement Elements { get; }

        public CarWashPage() {
            this.progressBar = new ProgressBar {
                MaxValue = 100,
                Value = 100,
                Width = 300,
                Height = 40,
            };

            this.timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            this.timer.Tick += this.OnTick;

            this.Elements = this.CreatePage();
        }

        public void Active() => this.timer.Start();
        public void Deactive() => this.timer.Stop();

        private UIElement CreatePage() {
            var label = new Text(this.font, "Washing your car...") {
                ForeColor = Colors.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Canvas.SetLeft(label, 140);
            Canvas.SetTop(label, 60);
            this.canvas.Children.Add(label);

            Canvas.SetLeft(this.progressBar, 90);
            Canvas.SetTop(this.progressBar, 80);
            this.canvas.Children.Add(this.progressBar);

            return this.canvas;
        }

        private void OnTick(object sender, EventArgs e) {
            this.progressBar.Value -= 10;
            this.progressBar.Invalidate();

            if (this.progressBar.Value > 0)
                return;

            this.timer.Stop();
            this.progressBar.Value = this.progressBar.MaxValue;

            Program.NavigateTo(Program.EndPage.Elements);
        }
    }
}
