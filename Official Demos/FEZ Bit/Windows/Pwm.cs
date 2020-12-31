using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class PwmWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This will test Pwm on two leds: ";
        private const string Instruction2 = "- Red led connect to PB0";
        private const string Instruction3 = "- Green led connect to PH11 ";
        private const string Instruction4 = " ";
        private const string Instruction5 = "Press Test button when you ready.";
        private const string Instruction6 = " ";
        private const string Instruction7 = " ";

        private const string Instruction8 = " ";

        private const string StatusPass1 = "The test is passed if red led is";
        private const string StatusPass2 = "changing brighness, and green is";
        private const string StatusPass3 = "blinking.";

        private Font font;

        private bool isRuning;

        private TextFlow textFlow;

        public PwmWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {

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

            this.textFlow.TextRuns.Add(Instruction6, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction7, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction8, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
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

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() {
            this.isRuning = false;

            Thread.Sleep(100); // Wait for test thread is stop => no update canvas
            // To stop or free, uinitialize variable resource

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

                        this.SetEnableButtonNext(false);

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
            //Because of https://github.com/ghi-electronics/TinyCLR-Libraries/issues/642
            // the green led PE11 is gpio for now.

            var gpioController = GpioController.GetDefault();

            var pwmController3 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20100.Timer.Pwm.Controller3.Id);
            //var pwmController1 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20100.Timer.Pwm.Controller1.Id);

            var pwmPinPB0 = pwmController3.OpenChannel(SC20100.Timer.Pwm.Controller3.PB0);
            //var pwmPinPE11 = pwmController1.OpenChannel(SC20100.Timer.Pwm.Controller1.PE11);
            var pwmPinPE11 = gpioController.OpenPin(SC20100.GpioPin.PE11);

            pwmPinPE11.SetDriveMode(GpioPinDriveMode.Output);

            pwmController3.SetDesiredFrequency(1000);
            //pwmController1.SetDesiredFrequency(1000);

            pwmPinPB0.SetActiveDutyCyclePercentage(0.0);
            //pwmPinPE11.SetActiveDutyCyclePercentage(0.0);

            var value = 0.0;
            var dir = 1;

            this.UpdateStatusText(StatusPass1, true);
            this.UpdateStatusText(StatusPass2, false);
            this.UpdateStatusText(StatusPass3, false);

            while (this.isRuning) {
                for (var i = 0; i < 10; i++) {
                    value += 0.1 * dir;

                    pwmPinPB0.Start();
                    //pwmPinPE11.Start();
                    pwmPinPE11.Write(pwmPinPE11.Read() == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);

                    Thread.Sleep(100);

                    pwmPinPB0.Stop();
                    //pwmPinPE11.Stop();

                    pwmPinPB0.SetActiveDutyCyclePercentage(value);
                    //pwmPinPE11.SetActiveDutyCyclePercentage(value);
                }

                dir = 0 - dir;
            }

            pwmPinPB0.Dispose();
            pwmPinPE11.Dispose();

            this.isRuning = false;

            return;
        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
