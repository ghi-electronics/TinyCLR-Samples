using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using FEZ_Portal.Properties;
using GHIElectronics.TinyCLR.UI.Media;

namespace FEZ_Portal {
    public sealed class SplashPage {
        private Canvas canvas;
        private Font font;
        private Font fontB;

        public UIElement Elements { get; }
        public SplashPage() {
            this.canvas = new Canvas();
            this.font = Resources.GetFont(Resources.FontResources.NinaB);
            this.fontB = Resources.GetFont(Resources.FontResources.ArialBlack);

            this.Elements = this.CreatePage();
        }
        private UIElement CreatePage() {

            var ghiLogo = new GHIElectronics.TinyCLR.UI.Controls.Image() {
                Source = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.logo)))
            };

            Canvas.SetLeft(ghiLogo, 0);
            Canvas.SetTop(ghiLogo, 0);
            this.canvas.Children.Add(ghiLogo);

            var buttonText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Enter") {
                ForeColor = Colors.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var enterButton = new Button() {
                Child = buttonText,
                Width = 65,
                Height = 40,
            };
            Canvas.SetLeft(enterButton, 200);
            Canvas.SetTop(enterButton, 230);
            this.canvas.Children.Add(enterButton);

            enterButton.Click += this.EnterButton_Click;

            return this.canvas;
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e) {

            if ("TouchUpEvent".CompareTo(e.RoutedEvent.Name) == 0) {
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {
                    Program.SelectPage = new SelectPage();
                    Program.WpfWindow.Child = Program.SelectPage.Elements;
                    Program.WpfWindow.Invalidate();
                    return null;
                }, null);
            }
        }

        public static SelectPage SelectPage { get; set; }
    }
}
