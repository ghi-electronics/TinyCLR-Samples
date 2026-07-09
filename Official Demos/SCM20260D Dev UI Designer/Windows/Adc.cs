using System;
using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // AdcContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same ADC test logic as before.
    public class AdcWindow : ApplicationWindow {
        private const string Instruction1 = " This will test Analog input on PA0C pin";
        private const string Instruction2 = " - Connect PA0C pin to analog source";
        private const string Instruction3 = " - The screen will show current value from the pin.";
        private const string Instruction4 = " ";
        private const string Instruction5 = " Press Test button when you are ready.";

        private readonly Font font;

        private Canvas canvas;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private bool isRunning;

        public AdcWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
        }

        protected override void Active() {
            var content = new AdcContent();
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

        private void ThreadTest() {
            this.isRunning = true;

            // Both controller and channel must come from the same board's pin
            // map. Original demo had SC20100 on the controller and SC20260 on
            // the channel — fixed to SC20260 throughout.
            var adc1 = AdcController.FromName(SC20260.Adc.Controller1.Id);
            var pin = adc1.OpenChannel(SC20260.Adc.Controller1.PA0C);

            var previous = string.Empty;
            while (this.isRunning) {
                var v = pin.ReadValue() * 3.3 / 0xFFFF;
                var s = v.ToString("N2");

                if (s.CompareTo(previous) != 0) {
                    this.UpdateStatusText("Adc reading value: " + s, true);
                    previous = s;
                }

                Thread.Sleep(500);
            }

            pin.Dispose();
            this.isRunning = false;
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(text, clearScreen, SystemDrawing.Color.White);

        private void UpdateStatusText(string text, bool clearScreen, SystemDrawing.Color color) {
            try {
                var expectedCount = this.textFlow.TextRuns.Count + 2;

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {
                    if (clearScreen)
                        this.textFlow.TextRuns.Clear();

                    this.textFlow.TextRuns.Add(text, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
                    this.textFlow.TextRuns.Add(TextRun.EndOfLine);
                    return null;
                }, null);

                var target = clearScreen ? 2 : expectedCount;
                while (this.textFlow.TextRuns.Count < target) {
                    Thread.Sleep(10);
                }
            }
            catch {
            }
        }
    }
}
