using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Dac;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class DacWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = " This will test Analog Out on: ";
        private const string Instruction2 = " - PA4: Value change from 0, 25, 50, 75 and 100% of 3.3V every second.";
        private const string Instruction3 = " - PA5: Value change from 100, 75, 50, 25 and 0% of 3.3V every second.";
        private const string Instruction4 = " - You may need an oscilloscope to measure the value on these pins.";
        private const string Instruction5 = " Press Test button when you are ready.";

        private readonly Button testButton;
        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;

        public DacWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
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
        }

        private void Initialize() {
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

            var controller = DacController.FromName(SC20260.Dac.Id);
            var channel0 = controller.OpenChannel(SC20260.Dac.PA4);
            var channel1 = controller.OpenChannel(SC20260.Dac.PA5);

            try {
                var pa4Values = new[] { 0.00, 0.25, 0.50, 0.75, 1.00 };
                var pa5Values = new[] { 1.00, 0.75, 0.50, 0.25, 0.00 };
                var labels = new[] {
                    "PA4: 0%. PA5: 100%",
                    "PA4: 25%. PA5: 75%",
                    "PA4: 50%. PA5: 50%",
                    "PA4: 75%. PA5: 25%",
                    "PA4: 100%. PA5: 0%",
                };

                while (this.isRunning) {
                    for (var i = 0; i < pa4Values.Length && this.isRunning; i++) {
                        channel0.WriteValue(pa4Values[i]);
                        channel1.WriteValue(pa5Values[i]);
                        this.UpdateStatusText(labels[i], true);
                        Thread.Sleep(1000);
                    }
                }
            }
            finally {
                channel0.Dispose();
                channel1.Dispose();
                this.isRunning = false;
            }
        }
    }
}
