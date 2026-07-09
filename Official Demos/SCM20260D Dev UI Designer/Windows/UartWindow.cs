using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // UartContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same UART5 echo test logic as before.
    public class UartWindow : ApplicationWindow {
        private const string Instruction1 = " This will test UART5 only: ";
        private const string Instruction2 = " - Connect UART5 to PC.";
        private const string Instruction3 = " - Open TeraTerm application.";
        private const string Instruction4 = " - Baudrate: 115200, DataBit 8, StopBit: One, Parity: None, ";
        private const string Instruction5 = "   Flow Control: None. ";
        private const string Instruction6 = " - Whatever you type on TeraTerm, SITCore will echo back the data + 1";
        private const string Instruction7 = " Example: Type 123... on TeraTerm, you will get back 234... ";
        private const string Instruction8 = " Press Test button when you are ready.";

        private readonly Font font;

        private Canvas canvas;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private bool isRunning;

        public UartWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
        }

        protected override void Active() {
            var content = new UartContent();
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

            using (var uart5 = UartController.FromName(SC20260.UartPort.Uart5)) {
                uart5.SetActiveSettings(new UartSetting { BaudRate = 115200 });
                uart5.Enable();

                var totalReceived = 0;
                var totalSent = 0;

                while (this.isRunning) {
                    this.UpdateStatusText("Total received: " + totalReceived, true);
                    this.UpdateStatusText("Total sent: " + totalSent, false);
                    this.UpdateStatusText("Listening data...", false);

                    while (uart5.BytesToRead == 0) {
                        Thread.Sleep(10);
                    }

                    var byteToRead = uart5.BytesToRead > uart5.ReadBufferSize ? uart5.ReadBufferSize : uart5.BytesToRead;
                    var read = new byte[byteToRead];

                    this.UpdateStatusText("Receiving... " + byteToRead + " byte(s)", false);
                    totalReceived += uart5.Read(read);

                    for (var i = 0; i < read.Length; i++) {
                        totalSent += uart5.Write(new byte[] { (byte)(read[i] + 1) });
                        uart5.Flush();
                    }

                    this.UpdateStatusText("Writing back... " + byteToRead + " byte(s)", false);
                }
            }

            this.isRunning = false;
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
