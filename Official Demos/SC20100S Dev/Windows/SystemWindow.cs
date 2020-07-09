using System;
using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class SystemWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = "Device: ";
        private const string Instruction2 = "Clock: 480 MHz";
        private const string Instruction3 = "Memory: 512KB Total";
        private const string Instruction4 = "External Memory: ";
        private const string Instruction5 = "OS: TinyCLR OS v";
        private const string Instruction6 = "Manufacture: ";

        public SystemWindow(Bitmap icon, string title, int width, int height) : base(icon, title, width, height) {

        }

        private Font font;
        private TextFlow textFlow;


        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);

            this.textFlow = new TextFlow();
            
            this.textFlow.TextRuns.Add(Instruction1 + GHIElectronics.TinyCLR.Native.DeviceInformation.DeviceName, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction2, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction3, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction4 + (GHIElectronics.TinyCLR.Native.Memory.UnmanagedMemory.FreeBytes / 1024) + "KB", this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction5 + GHIElectronics.TinyCLR.Native.DeviceInformation.Version.ToString("x8").Substring(0, 3), this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction6 + GHIElectronics.TinyCLR.Native.DeviceInformation.ManufacturerName, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

        }

        private void Deinitialize() {

            this.textFlow.TextRuns.Clear();
            this.textFlow = null;

            this.font.Dispose();

        }

        private void ClearScreen() {
            this.canvas.Children.Clear();

            // Enable TopBar
            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0); Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            // Enable BottomBar - If needed
            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0); Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                // Regiter touch event for button back or next
                // Regiter Button event
                this.OnBottomBarButtonUpEvent += this.TemplateWindow_OnBottomBarButtonUpEvent;
            }

        }

        private void TemplateWindow_OnBottomBarButtonUpEvent(object sender, RoutedEventArgs e) {
            var buttonSource = (GHIElectronics.TinyCLR.UI.Input.ButtonEventArgs)e;

            switch (buttonSource.Button) {
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Left:
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Right:
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select:
                    // close this window, back to previous window ???
                    this.Close();
                    break;

            }
        }

        private void CreateWindow() {
            var startX = 5;
            var startY = 20;

            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);            
        }      
        

        protected override void Active() {
            this.Initialize();

            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.ClearScreen();

            this.CreateWindow();
        }

        protected override void Deactive() {
            this.canvas.Children.Clear();

            this.Deinitialize();
        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) {

            var timeout = 100;
            var count = this.textFlow.TextRuns.Count + 2;

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(timeout), _ => {

                if (clearscreen)
                    this.textFlow.TextRuns.Clear();

                this.textFlow.TextRuns.Add(text, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                return null;

            }, null);

            if (clearscreen) {
                while (this.textFlow.TextRuns.Count < 2) {
                    Thread.Sleep(1);
                }
            }
            else {
                while (this.textFlow.TextRuns.Count < count) {
                    Thread.Sleep(1);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();


        }

    }
}
