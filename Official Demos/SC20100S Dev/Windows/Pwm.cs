using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class PwmWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;

        private const string Instruction1 = "This will test PWM on two leds:";
        private const string Instruction2 = "- Red led connected to PB0";
        private const string Instruction3 = "- Green led connected to PE11";
        private const string Instruction4 = " ";
        private const string Instruction5 = "Press Test when you are ready.";

        private const string StatusPass1 = "The test passes if red is";
        private const string StatusPass2 = "changing brightness and green";
        private const string StatusPass3 = "is blinking.";

        public PwmWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
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
            // Green LED on PE11 is toggled as plain GPIO rather than PWM —
            // see TinyCLR-Libraries#642 (Timer.Pwm.Controller1 channel mapping
            // for PE11 conflicts with the display SPI clock pin on this board).
            this.isRunning = true;

            var gpioController = GpioController.GetDefault();
            var pwmController3 = PwmController.FromName(SC20100.Timer.Pwm.Controller3.Id);
            var pwmPinPB0 = pwmController3.OpenChannel(SC20100.Timer.Pwm.Controller3.PB0);
            var greenLed = gpioController.OpenPin(SC20100.GpioPin.PE11);

            greenLed.SetDriveMode(GpioPinDriveMode.Output);
            pwmController3.SetDesiredFrequency(1000);
            pwmPinPB0.SetActiveDutyCyclePercentage(0.0);

            this.UpdateStatusText(StatusPass1, true);
            this.UpdateStatusText(StatusPass2, false);
            this.UpdateStatusText(StatusPass3, false);

            var value = 0.0;
            var dir = 1;

            while (this.isRunning) {
                for (var i = 0; i < 10; i++) {
                    value += 0.1 * dir;

                    pwmPinPB0.Start();
                    greenLed.Write(greenLed.Read() == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);

                    Thread.Sleep(100);

                    pwmPinPB0.Stop();
                    pwmPinPB0.SetActiveDutyCyclePercentage(value);
                }

                dir = -dir;
            }

            pwmPinPB0.Dispose();
            greenLed.Dispose();
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
