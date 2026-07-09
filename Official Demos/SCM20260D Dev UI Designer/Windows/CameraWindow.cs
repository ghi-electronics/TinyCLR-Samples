using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.Omnivision.Ov9655;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The static layout (status TextFlow + Test button) is designed in
    // CameraContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same camera test logic as before.
    // NOTE: the live camera frames are captured and blitted straight to the display via
    // DisplayController.DrawBuffer in ThreadTest — that raw drawing is not a UI control and stays in code.
    public class CameraWindow : ApplicationWindow {
        private const string Instruction1 = " This will test Camera module: ";
        private const string Instruction2 = " - Connect Camera module to Camera Interface on the 20260Dev board.";
        private const string Instruction3 = " Press Test button when you are ready.";

        private readonly Font font;

        private Canvas canvas;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private bool isRunning;

        public CameraWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
        }

        protected override void Active() {
            var content = new CameraContent();
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

            var i2cController = I2cController.FromName(SC20260.I2cBus.I2c1);

            Ov9655Controller ov9655 = null;
            const int retries = 2; // some camera may fail to initialize after reset the first time

            for (var i = 0; i < retries; i++) {
                try {
                    ov9655 = new Ov9655Controller(i2cController);
                    ov9655.SetResolution(Ov9655Controller.Resolution.Vga);
                    break;
                }
                catch {
                }
            }

            if (ov9655 == null) {
                this.isRunning = false;
                return;
            }

            var displayController = Display.DisplayController;

            while (this.isRunning) {
                try {
                    ov9655.Capture();
                    displayController.DrawBuffer(0, this.TopBar.ActualHeight, 0, 0, 480, 272 - this.TopBar.ActualHeight, 640, ov9655.Buffer, 0);
                }
                catch {
                }

                Thread.Sleep(10);
            }

            this.isRunning = false;
        }
    }
}
