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
    // BuzzerContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same PWM test logic as before.
    public class BuzzerWindow : ApplicationWindow {
        private const string Instruction1 = "This test Buzzer:";
        private const string Instruction2 = " First second: generate PWM  500Hz, duty cycle 0.5";
        private const string Instruction3 = " Next second:  generate PWM 1000Hz, duty cycle 0.5";
        private const string Instruction4 = " Third second: generate PWM 2000Hz, duty cycle 0.5";
        private const string Instruction5 = "Press Test button when you are ready.";

        private readonly SystemDrawing.Font font;

        private Canvas canvas;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private bool isRunning;

        public BuzzerWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
        }

        protected override void Active() {
            var content = new BuzzerContent();
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

                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (this.isRunning)
                return;

            this.testButton.Visibility = Visibility.Collapsed; // hide while running (original removed it)
            this.textFlow.TextRuns.Clear();
            new Thread(this.ThreadTest).Start();
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
        }

        private void ThreadTest() {
            this.isRunning = true;

            using (var pwmController3 = PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id)) {
                var pwmPinPB1 = pwmController3.OpenChannel(SC20260.Timer.Pwm.Controller3.PB1);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.UpdateStatusText("Generate Pwm 500Hz...", true);
                pwmController3.SetDesiredFrequency(500);
                pwmPinPB1.Start();
                Thread.Sleep(1000);
                pwmPinPB1.Stop();

                this.UpdateStatusText("Generate Pwm 1000Hz...", false);
                pwmController3.SetDesiredFrequency(1000);
                pwmPinPB1.Start();
                Thread.Sleep(1000);
                pwmPinPB1.Stop();

                this.UpdateStatusText("Generate Pwm 2000Hz...", false);
                pwmController3.SetDesiredFrequency(2000);
                pwmPinPB1.Start();
                Thread.Sleep(1000);
                pwmPinPB1.Stop();

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Test passes if you heard three different tones!", false);
            }

            this.isRunning = false;
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
