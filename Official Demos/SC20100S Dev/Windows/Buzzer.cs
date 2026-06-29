using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class BuzzerWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;

        private const string Instruction1 = "This test Buzzer:";
        private const string Instruction2 = " First sound at 500Hz";
        private const string Instruction3 = " Second sound at 1000Hz";
        private const string Instruction4 = " Third sound at 2000Hz";
        private const string Instruction5 = "Press Test button when you are ready.";

        public BuzzerWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
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
            Thread.Sleep(10);
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

            using (var pwmController3 = PwmController.FromName(SC20100.Timer.Pwm.Controller3.Id)) {
                var pwmPinPB1 = pwmController3.OpenChannel(SC20100.Timer.Pwm.Controller3.PB1);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.PlayTone(pwmController3, pwmPinPB1, 500,  "Generate Pwm 500Hz...",  true);
                this.PlayTone(pwmController3, pwmPinPB1, 1000, "Generate Pwm 1000Hz...", false);
                this.PlayTone(pwmController3, pwmPinPB1, 2000, "Generate Pwm 2000Hz...", false);

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Test passes if you heard three", false);
                this.UpdateStatusText("different tones!", false);
            }

            this.isRunning = false;
        }

        private void PlayTone(PwmController controller, PwmChannel channel, int frequency, string status, bool clearScreen) {
            this.UpdateStatusText(status, clearScreen);
            controller.SetDesiredFrequency(frequency);
            channel.Start();
            Thread.Sleep(1000);
            channel.Stop();
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
