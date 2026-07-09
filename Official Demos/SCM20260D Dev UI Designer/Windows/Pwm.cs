using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // PwmContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same PWM test logic as before.
    public class PwmWindow : ApplicationWindow {
        private const string Instruction1 = " This will test PWM on two leds: ";
        private const string Instruction2 = " - Red led connected to PB0.";
        private const string Instruction3 = " - Green led connected to PH11.";
        private const string Instruction4 = " Press Test button when you are ready.";

        private readonly Font font;

        private Canvas canvas;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private bool isRunning;

        public PwmWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
        }

        protected override void Active() {
            var content = new PwmContent();
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
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);

        private void ThreadTest() {
            this.isRunning = true;

            var pwmController3 = PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id);
            var pwmController5 = PwmController.FromName(SC20260.Timer.Pwm.Controller5.Id);

            var pwmPinPB0 = pwmController3.OpenChannel(SC20260.Timer.Pwm.Controller3.PB0);
            var pwmPinPH11 = pwmController5.OpenChannel(SC20260.Timer.Pwm.Controller5.PH11);

            pwmController3.SetDesiredFrequency(1000);
            pwmController5.SetDesiredFrequency(1000);

            pwmPinPB0.SetActiveDutyCyclePercentage(0.0);
            pwmPinPH11.SetActiveDutyCyclePercentage(0.0);

            this.UpdateStatusText("The test passes if red and green led are changing brightness.", true);

            var value = 0.0;
            var dir = 1;

            while (this.isRunning) {
                for (var i = 0; i < 10; i++) {
                    value += 0.1 * dir;

                    pwmPinPB0.Start();
                    pwmPinPH11.Start();

                    Thread.Sleep(100);

                    pwmPinPB0.Stop();
                    pwmPinPH11.Stop();

                    pwmPinPB0.SetActiveDutyCyclePercentage(value);
                    pwmPinPH11.SetActiveDutyCyclePercentage(value);
                }

                dir = -dir;
            }

            pwmPinPB0.Dispose();
            pwmPinPH11.Dispose();
            this.isRunning = false;
        }
    }
}
