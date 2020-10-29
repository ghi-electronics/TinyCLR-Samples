using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Drivers.Omnivision.Ov9655;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class DacWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel
      
        private const string Instruction1 = " This will test Analog Out on: ";
        private const string Instruction2 = " - PA4: Value change from 0, 25, 50, 75 and 100% of 3.3V every second.";
        private const string Instruction3 = " - PA5: Value change from 100, 55, 50, 25 and 0% of 3.3V every second.";
        private const string Instruction4 = " - You may need a oscilloscope to measure the value on these pins. ";
        private const string Instruction5 = "  ";
        private const string Instruction6 = "  Press Test button when you are ready.";
        private const string Instruction7 = "  ";

        private const string Instruction8 = " ";

        private Button testButton;

        private Font font;

        private bool isRunning;

        private TextFlow textFlow;

        public DacWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.testButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Test") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;

        }

        private void Initialize() {

            this.textFlow = new TextFlow();

            this.textFlow.TextRuns.Add(Instruction1, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction2, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction3, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction4, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction5, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction6, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {

            this.textFlow.TextRuns.Clear();
            this.textFlow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {

                if (!this.isRunning) {
                    this.ClearScreen();

                    this.CreateWindow(false);

                    this.textFlow.TextRuns.Clear();

                    new Thread(this.ThreadTest).Start();
                }
            }
        }


        protected override void Active() {
            // To initialize, reset your variable, design...
            this.Initialize();

            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.isRunning = false;

            this.ClearScreen();
            this.CreateWindow(true);
        }

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() {
            this.isRunning = false;

            Thread.Sleep(10);
            // To stop or free, uinitialize variable resource
            this.canvas.Children.Clear();

            this.Deinitialize();
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
                this.OnBottomBarButtonBackTouchUpEvent += this.TemplateWindow_OnBottomBarButtonBackTouchUpEvent;
                this.OnBottomBarButtonNextTouchUpEvent += this.TemplateWindow_OnBottomBarButtonNextTouchUpEvent;
            }

        }

        private void CreateWindow(bool enablebutton) {
            var startX = 5;
            var startY = 40;            

            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);

            if (enablebutton) {
                var buttonY = this.Height - ((this.testButton.Height * 3) / 2);

                Canvas.SetLeft(this.testButton, startX); Canvas.SetTop(this.testButton, buttonY);
                this.canvas.Children.Add(this.testButton);
            }

           
        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);

        private void ThreadTest() {

            this.isRunning = true;

            var controlelr = GHIElectronics.TinyCLR.Devices.Dac.DacController.FromName(SC20100.Dac.Id);

            var channel0 = controlelr.OpenChannel(0);
            var channel1 = controlelr.OpenChannel(1);

            while (this.isRunning) {
              
                channel0.WriteValue(0);
                channel1.WriteValue(1.0);
                this.UpdateStatusText("PA4: 0%. PA5: 100%",  true);

                var t = DateTime.Now;

                while ((DateTime.Now - t).TotalMilliseconds < 1000) {
                    if (this.isRunning == false)
                        break;
                }

                channel0.WriteValue(0.25);
                channel1.WriteValue(0.75);
                this.UpdateStatusText("PA4: 25%. PA5: 75%",  true);
                t = DateTime.Now;

                while ((DateTime.Now - t).TotalMilliseconds < 1000) {
                    if (this.isRunning == false)
                        break;
                }

                channel0.WriteValue(0.5);
                channel1.WriteValue(0.5);
                this.UpdateStatusText("PA4: 50%. PA5: 50%",  true);
                t = DateTime.Now;

                while ((DateTime.Now - t).TotalMilliseconds < 1000) {
                    if (this.isRunning == false)
                        break;
                }

                channel0.WriteValue(0.75);
                channel1.WriteValue(0.25);
                this.UpdateStatusText("PA4: 75%. PA5: 25%",  true);
                t = DateTime.Now;

                while ((DateTime.Now - t).TotalMilliseconds < 1000) {
                    if (this.isRunning == false)
                        break;
                }

                channel0.WriteValue(1.0);
                channel1.WriteValue(0);
                this.UpdateStatusText("PA4: 100%. PA5: 0%",  true);
                t = DateTime.Now;

                while ((DateTime.Now - t).TotalMilliseconds < 1000) {
                    if (this.isRunning == false)
                        break;
                }

            }

            channel0.Dispose();
            channel1.Dispose();

            this.isRunning = false;

            return;

        }
       
    }
}
