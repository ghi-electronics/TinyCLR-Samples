using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class NetworkWindow : ApplicationWindow {
        // SystemDrawing.Color is used for the section-header tint; UI.Media
        // .Colors covers the body text. The two namespaces both define Color,
        // so we qualify the System.Drawing one to keep the names unambiguous.
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;

        private const string WifiHeader = "Wifi: supports Winc15xx:";
        private const string WifiUrl1 = "- http://docs.ghielectronics.com";
        private const string WifiUrl2 = "/software/tinyclr/tutorials/wifi.html";
        private const string EthHeader = "Ethernet: support ENC28J60: ";
        private const string EthUrl1 = "- http://docs.ghielectronics.com";
        private const string EthUrl2 = "/software/tinyclr/tutorials";
        private const string EthUrl3 = "/ethernet.html";

        public NetworkWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();

            this.AppendLine(WifiHeader, SystemDrawing.Color.FromArgb(0, 0, 0xFF));
            this.AppendLine(WifiUrl1);
            this.AppendLine(WifiUrl2);
            this.AppendLine(" ");
            this.AppendLine(EthHeader, SystemDrawing.Color.FromArgb(0, 0, 0xFF));
            this.AppendLine(EthUrl1);
            this.AppendLine(EthUrl2);
            this.AppendLine(EthUrl3);
        }

        private void AppendLine(string text) => this.AppendLine(text, SystemDrawing.Color.White);

        private void AppendLine(string text, SystemDrawing.Color color) {
            this.textFlow.TextRuns.Add(text, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {
            if (this.BottomBar != null)
                this.OnBottomBarButtonUpEvent -= this.OnHardwareButtonUp;

            this.textFlow.TextRuns.Clear();
            this.canvas.Children.Clear();
            this.font.Dispose();
            this.textFlow = null;
            this.canvas = null;
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.ClearScreen();
            this.CreateWindow();
        }

        protected override void Deactive() => this.Deinitialize();

        private void ClearScreen() {
            this.canvas.Children.Clear();

            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);
                this.OnBottomBarButtonUpEvent += this.OnHardwareButtonUp;
            }
        }

        private void OnHardwareButtonUp(object sender, RoutedEventArgs e) =>
            // Info-only window. Any button closes back to the main menu.
            this.Close();

        private void CreateWindow() {
            Canvas.SetLeft(this.textFlow, 5);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }
    }
}
