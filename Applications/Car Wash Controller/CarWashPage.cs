using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using CarWashExample.Properties;

namespace CarWashExample
{
    public sealed class CarWashPage
    {
        private Canvas canvas;        
        private Font fontB;
        private DispatcherTimer timer;
        private ProgressBar progressBar;

        public UIElement Elements { get; }

        public CarWashPage()
        {
            this.canvas = new Canvas();
            this.fontB = Resources.GetFont(Resources.FontResources.NinaB);

            this.progressBar = new ProgressBar()
            {
                MaxValue = 100,
                Value = 100,
                Width = 300,
                Height = 40
            };

            this.timer = new DispatcherTimer {
                Tag = this.progressBar
            };
            this.timer.Tick += this.Counter;
            this.timer.Interval = new TimeSpan(0, 0, 1);

            this.Elements = this.CreatePage();
        }

        public void Active() => this.timer.Start();

        public void Deactive() => this.timer.Stop();

        private UIElement CreatePage()
        {

            this.canvas.Children.Clear();

            var washCarText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.fontB, "Washing your car...")
            {
                ForeColor = Colors.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Canvas.SetLeft(washCarText, 140);
            Canvas.SetTop(washCarText, 60);

            this.canvas.Children.Add(washCarText);

            Canvas.SetLeft(this.progressBar, 90);
            Canvas.SetTop(this.progressBar, 80);

            this.canvas.Children.Add(this.progressBar);

            return this.canvas;

        }

        void Counter(object sender, EventArgs e)
        {
            this.progressBar.Value -= 10;
            this.progressBar.Invalidate();

            if (this.progressBar.Value <= 0)
            {
                this.timer.Stop();
                this.progressBar.Value = this.progressBar.MaxValue;

                Program.WpfWindow.Child = Program.EndPage.Elements;
                Program.WpfWindow.Invalidate();
            }

        }
    }
}
