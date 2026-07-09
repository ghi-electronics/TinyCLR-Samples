using System;
using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // RtcContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same RTC test logic as before.
    public class RtcWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = " *** Be careful: this test will enable charging mode on the VBAT pin. ***";
        private const string Instruction2 = " This will test RTC. The time will start at 00:00:00 - 07/07/2020.";
        private const string Instruction3 = " - Wait for charging about 30 seconds.";
        private const string Instruction4 = " - Power off the board for 10 seconds.";
        private const string Instruction5 = " - Power on the board.";
        private const string Instruction6 = " => Passed if timer is after 00:00:00 - 07/07/2020.";
        private const string Instruction7 = "    Failed if timer is reset to 00:00:00 - 01/01/2017.";
        private const string Instruction8 = " Press Test button when you are ready.";

        private const int ChargeVbatTimeoutSeconds = 30;

        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment

        private readonly RtcController rtc;

        public RtcWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.rtc = RtcController.GetDefault();
            this.rtc.SetChargeMode(BatteryChargeMode.None);
        }

        protected override void Active() {
            var content = new RtcContent();
            this.canvas = content;
            this.textFlow = content.StatusText;
            this.testButton = content.TestButton;
            this.testButton.Click += this.TestButton_Click;
            this.isRunning = false;

            this.AddChrome();
            this.Child = this.canvas;

            this.textFlow.TextRuns.Clear();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
            this.AppendInstruction(Instruction6);
            this.AppendInstruction(Instruction7);
            this.AppendInstruction(Instruction8);
        }

        // Adds the shared TopBar/BottomBar chrome (unchanged from the original) to the fragment's canvas.
        private void AddChrome() {
            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                // Register touch event for Back / Next.
                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {
            this.textFlow.TextRuns.Clear();
            this.textFlow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (!this.isRunning) {
                this.testButton.Visibility = Visibility.Collapsed; // hide while running (original removed it)
                this.textFlow.TextRuns.Clear();
                new Thread(this.ThreadTest).Start();
            }
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
            this.Deinitialize();
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);

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

                var newDt = RtcDateTime.FromDateTime(reference);
                this.rtc.SetTime(newDt);
                SystemTime.SetTime(reference);

                this.UpdateStatusText("Please power off the board for 10 seconds.", false);
            }

            this.isRunning = false;
        }
    }
}
