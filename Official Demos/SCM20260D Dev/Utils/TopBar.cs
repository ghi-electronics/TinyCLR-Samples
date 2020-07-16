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

        private readonly bool enableClock;
        private readonly Button buttonClose;

        private readonly Font font;

        private readonly int width;
        private readonly int height;

        private DispatcherTimer clockTimer;

        public delegate void OnCloseEventHandle(object sender, RoutedEventArgs e);
        public event OnCloseEventHandle OnClose;

        private Canvas canvas;

        public UIElement Child { get; private set; }

        public TopBar(int width, string leftText, bool enableClock = false) {
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

            this.enableClock = enableClock;

            if (this.enableClock == false) { // then enable button

                this.buttonClose = new Button() {
                    Child = closeText,
                    Width = this.height,
                    Height = this.height,
                };
            }


            this.CreateBar();
            this.Child.IsVisibleChanged += this.Element_IsVisibleChanged;
        }

        private void Element_IsVisibleChanged(object sender, PropertyChangedEventArgs e) => this.CreateClockTimer();

        private void CreateBar() {

            var rect = new GHIElectronics.TinyCLR.UI.Shapes.Rectangle(this.width, this.height) {
                Fill = new LinearGradientBrush(GHIElectronics.TinyCLR.UI.Media.Color.FromArgb(0xff, 0xc4, 0x83, 0x41), GHIElectronics.TinyCLR.UI.Media.Color.FromArgb(0xff, 0x66, 0x44, 0x22), 0, 0, this.width, this.height),
                //Fill = new SolidColorBrush(GHIElectronics.TinyCLR.UI.Media.Color.FromArgb(0xff, 0xc4, 0x83, 0x41)),// Colors.Black,
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

            if (this.enableClock) {
                this.rightLabel = new Text {
                    ForeColor = Colors.White,
                    Font = font,
                    TextContent = "",
                };

                Canvas.SetRight(this.rightLabel, 0);
                Canvas.SetTop(this.rightLabel, 2);
                this.canvas.Children.Add(this.rightLabel);

            }
            else {
                Canvas.SetRight(this.buttonClose, 0);
                Canvas.SetTop(this.buttonClose, 0);

                this.canvas.Children.Add(this.buttonClose);

                this.buttonClose.Click += this.ButtonClose_Click;
            }

            this.Child = this.canvas;
        }

        private void CreateClockTimer() {
            if (this.enableClock && this.clockTimer == null) {
                this.clockTimer = new DispatcherTimer();

                this.clockTimer.Tick += this.ClockTimer_Tick;
                this.clockTimer.Interval = new TimeSpan(0, 0, 1);
                this.clockTimer.Start();
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e) {
            if (this.enableClock)
                this.rightLabel.TextContent = DateTime.Now.ToString();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) {
            if (this.OnClose != null) {
                if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                    this.OnClose?.Invoke(sender, e);
                }
            }
        }

        public void Dispose() {
            this.canvas.Children.Clear();

            this.font.Dispose();

            if (this.buttonClose != null)
                this.buttonClose.Dispose();
        }
    }
}
