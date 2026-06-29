using System;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class RtcWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;

        private readonly RtcController rtc;
        private const int ChargeVbatTimeoutSeconds = 30;

        private const string Instruction1 = " *** Be careful: this test will";
        private const string Instruction2 = " enable charging on VBAT pin. ***";
        private const string Instruction3 = " ";
        private const string Instruction4 = " Press Test when you are ready.";

        public RtcWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
            this.rtc = RtcController.GetDefault();
            this.rtc.SetChargeMode(BatteryChargeMode.None);
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
        }

        private void AppendInstruction(string text) {
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

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.ClearScreen();
            this.CreateWindow();
        }

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(100);
            this.Deinitialize();
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
            var buttonSource = (ButtonEventArgs)e;
            switch (buttonSource.Button) {
                case HardwareButton.Left:
                    this.Close();
                    break;
                case HardwareButton.Right:
                case HardwareButton.Select:
                    if (!this.isRunning)
                        new Thread(this.ThreadTest).Start();
                    break;
            }
        }

        private void CreateWindow() {
            Canvas.SetLeft(this.textFlow, 5);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }

        private void ThreadTest() {
            this.isRunning = true;
            var reference = new DateTime(2020, 7, 7, 0, 0, 0);

            if (this.rtc.IsValid && this.rtc.Now >= reference) {
                while (this.isRunning) {
                    this.UpdateStatusText("RTC is working: " + this.rtc.Now, true);
                    Thread.Sleep(1000);
                }
            }
            else {
                this.rtc.SetChargeMode(BatteryChargeMode.Fast);
                for (var elapsed = 0; elapsed < ChargeVbatTimeoutSeconds && this.isRunning; elapsed++) {
                    this.UpdateStatusText("Please wait for charging.... " + elapsed + " / " + ChargeVbatTimeoutSeconds, true);
                    Thread.Sleep(1000);
                }
                this.rtc.SetChargeMode(BatteryChargeMode.None);

                this.rtc.SetTime(RtcDateTime.FromDateTime(reference));
                SystemTime.SetTime(reference);

                this.UpdateStatusText("Please power off the board for 10", false);
                this.UpdateStatusText("seconds, then check RTC timer again", false);
            }

            this.isRunning = false;
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
