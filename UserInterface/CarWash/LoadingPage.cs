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
    public sealed class LoadingPage
    {
        private Canvas canvas;        
        private Font fontB;
        private DispatcherTimer timer;
        private ProgressBar progressBar;

        public UIElement Elements { get; }

        public LoadingPage()
        {
            this.canvas = new Canvas();            
            fontB = Resources.GetFont(Resources.FontResources.NinaB);

            progressBar = new ProgressBar()
            {
                MaxValue = 100,
                Value = 0,
                Width = 200,
                Height = 20
            };

            timer = new DispatcherTimer();

            timer.Tag = progressBar;
            timer.Tick += Counter;
            timer.Interval = new TimeSpan(0, 0, 1);

            this.Elements = this.CreatePage();
        }
        public void Active()
        {
            timer.Start();
        }

        public void Deactive()
        {
            timer.Stop();
        }

        private UIElement CreatePage()
        {
            this.canvas.Children.Clear();

            var loadingText = new GHIElectronics.TinyCLR.UI.Controls.Text(fontB, "Processing your payment...")
            {
                ForeColor = Colors.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Canvas.SetLeft(loadingText, 140);
            Canvas.SetTop(loadingText, 220);

            this.canvas.Children.Add(loadingText);

            Canvas.SetLeft(progressBar, 140);
            Canvas.SetTop(progressBar, 240);

            this.canvas.Children.Add(progressBar);

            return this.canvas;

        }

        void Counter(object sender, EventArgs e)
        {
            progressBar.Value += 10;
            progressBar.Invalidate();

            if (progressBar.Value == progressBar.MaxValue)
            {
                timer.Stop();

                progressBar.Value = 0;

                Program.WpfWindow.Child = Program.CarWashPage.Elements;
                Program.WpfWindow.Invalidate();

                Program.CarWashPage.Active();
            }

        }
    }
}
