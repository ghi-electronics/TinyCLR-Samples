using System.Drawing;
using Demos.Properties;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    internal class SystemWindow : ApplicationWindow {
        private Canvas canvas;
        private Font font;
        private TextFlow textFlow;

        private const string DemoVersion = "052126"; // May-21-2026

        public SystemWindow(Resources.BitmapResources icon, string title, int width, int height) : base(icon, title, width, height) {
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();

            this.AppendLine("Device: " + DeviceInformation.DeviceName);
            this.AppendLine("Clock: " + (Power.GetSystemClock() == SystemClock.High ? "480MHz" : "240MHz"));
            this.AppendLine("Memory: 512KB Total");
            this.AppendLine("External Memory: " + (Memory.UnmanagedMemory.FreeBytes / 1024) + "KB");
            this.AppendLine("OS: TinyCLR OS v" + DeviceInformation.Version.ToString("x8").Substring(0, 3));
            this.AppendLine("Manufacture: " + DeviceInformation.ManufacturerName);
            this.AppendLine("Demo version: " + DemoVersion);
        }

        private void AppendLine(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
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

        private void OnHardwareButtonUp(object sender, RoutedEventArgs e) {
            // Any of Left / Right / Select closes back to the main menu.
            this.Close();
        }

        private void CreateWindow() {
            Canvas.SetLeft(this.textFlow, 5);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.ClearScreen();
            this.CreateWindow();
        }

        protected override void Deactive() => this.Deinitialize();
    }
}
