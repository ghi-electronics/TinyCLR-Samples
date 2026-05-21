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

        private readonly Button testButton;
        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;

        private readonly RtcController rtc;

        public RtcWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.testButton = new Button() {
                Child = new Text(this.font, "Test") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;

            this.rtc = RtcController.GetDefault();
            this.rtc.SetChargeMode(BatteryChargeMode.None);
        }

        private void Initialize() {
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
            this.AppendInstruction(Instruction6);
            this.AppendInstruction(Instruction7);
            this.AppendInstruction(Instruction8);
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
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0 && !this.isRunning) {
                this.ClearScreen();
                this.CreateWindow(false);
                this.textFlow.TextRuns.Clear();
                new Thread(this.ThreadTest).Start();
            }
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.isRunning = false;
            this.ClearScreen();
            this.CreateWindow(true);
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
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

                // Register touch event for Back / Next.
                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void CreateWindow(bool enableButton) {
            const int startX = 5;
            const int startY = 40;

            Canvas.SetLeft(this.textFlow, startX);
            Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);

            if (enableButton) {
                var buttonY = this.Height - ((this.testButton.Height * 3) / 2);
                Canvas.SetLeft(this.testButton, startX);
                Canvas.SetTop(this.testButton, buttonY);
                this.canvas.Children.Add(this.testButton);
            }
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
