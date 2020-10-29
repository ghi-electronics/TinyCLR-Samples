using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class BuzzerWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This test Buzzer:";
        private const string Instruction2 = " First sound at 500Hz";
        private const string Instruction3 = " Second sound at 1000";
        private const string Instruction4 = " Third sound at 2000Hz";
        private const string Instruction5 = "Press Test button when you ready.";

        private Font font;
        private TextFlow textFlow;
        private bool isRuning;

        public BuzzerWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {


        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);

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

        }

        private void Deinitialize() {
            if (this.BottomBar != null) {
                this.OnBottomBarButtonUpEvent -= this.TemplateWindow_OnBottomBarButtonUpEvent;
            }

            this.textFlow.TextRuns.Clear();
            this.canvas.Children.Clear();

            this.font.Dispose();

            this.textFlow = null;
            this.canvas = null;
        }

        protected override void Active() {
            // To initialize, reset your variable, design...
            this.Initialize();

            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.ClearScreen();
            this.CreateWindow();
        }

        protected override void Deactive() =>
            // To stop or free, uinitialize variable resource
            this.Deinitialize();

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
                    // close this window, back to previous window ???
                    this.Close();
                    break;

                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Right:
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select:
                    if (this.isRuning == false) {
                        new Thread(this.ThreadTest).Start();
                    }
                    break;


            }
        }

        private void CreateWindow() {
            var startX = 5;
            var startY = 20;

            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);
        }


        private void ThreadTest() {

            this.isRuning = true;

            using (var pwmController3 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id)) {

                var pwmPinPB1 = pwmController3.OpenChannel(SC20260.Timer.Pwm.Controller3.PB1);

                pwmController3.SetDesiredFrequency(500);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.UpdateStatusText("Generate Pwm 500Hz...", true);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                this.UpdateStatusText("Generate Pwm 1000Hz...", false);

                pwmController3.SetDesiredFrequency(1000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                this.UpdateStatusText("Generate Pwm 2000Hz...", false);

                pwmController3.SetDesiredFrequency(2000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Testing is success if you heard three", false);
                this.UpdateStatusText("kind of sounds!", false);
            }

            this.isRuning = false;

            return;

        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
