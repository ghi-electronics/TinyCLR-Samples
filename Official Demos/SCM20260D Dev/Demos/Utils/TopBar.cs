using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public class TopBar {

        private Text leftLabel;
        private Text rightLabel;

        public string LeftText { get; set; }        

        private readonly bool enableCloseButton;
        private readonly Button buttonClose;

        private readonly Font font;

        private readonly int width;
        private readonly int height;

        private DispatcherTimer clockTimer;

        public delegate void OnCloseEventHandle(object sender, RoutedEventArgs e);
        public event OnCloseEventHandle OnClose;

        private readonly Canvas canvas;

        public UIElement Element { get; private set; }

        public TopBar(int width, string leftText, bool enableCloseButton = true) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.height = this.font.Height + 8;
            this.width = width;

            this.canvas = new Canvas {
                Width = this.width,
                Height = this.height
            };

            this.LeftText = leftText;

            var closeText = new Text(this.font, "X") {
                ForeColor = Colors.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            if (enableCloseButton) {
                this.enableCloseButton = enableCloseButton;

                this.buttonClose = new Button() {
                    Child = closeText,
                    Width = this.height,
                    Height = this.height,
                };
            }
            else {
                this.enableCloseButton = false;
            }

            this.CreateBar();
            this.CreateClockTimer();
        }

        private void CreateBar() {

            var rect = new GHIElectronics.TinyCLR.UI.Shapes.Rectangle(this.width, this.height) {
                Fill = new SolidColorBrush(Colors.Green),
            };

            this.canvas.Children.Add(rect);

            this.leftLabel = new Text {
                ForeColor = Colors.White,
                Font = font,
                TextContent = this.LeftText,
            };

            Canvas.SetLeft(this.leftLabel, 0);
            Canvas.SetTop(this.leftLabel, 2);
            this.canvas.Children.Add(this.leftLabel);

            if (this.enableCloseButton) {
                Canvas.SetRight(this.buttonClose, 0);
                Canvas.SetTop(this.buttonClose, 0);

                this.canvas.Children.Add(this.buttonClose);

                this.buttonClose.Click += this.ButtonClose_Click;
            }
            else {
                this.rightLabel = new Text {
                    ForeColor = Colors.White,
                    Font = font,
                    TextContent = "",
                };

                Canvas.SetRight(this.rightLabel, 0);
                Canvas.SetTop(this.rightLabel, 2);
                this.canvas.Children.Add(this.rightLabel);
            }

            this.Element = this.canvas;
        }

        private void CreateClockTimer() {
            if (this.enableCloseButton == false) {
                this.clockTimer = new DispatcherTimer();

                this.clockTimer.Tick += this.ClockTimer_Tick;
                this.clockTimer.Interval = new TimeSpan(0, 0, 1);
                this.clockTimer.Start();
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e) {
            if (this.enableCloseButton == false)
                this.rightLabel.TextContent = DateTime.Now.ToString();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) {
            if (this.OnClose != null) {
                if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                    this.OnClose?.Invoke(sender, e);
                }                
            }
        }
    }
}
