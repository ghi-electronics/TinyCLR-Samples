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
    public class UartWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = " This will test UART5 only: ";
        private const string Instruction2 = " - Connect UART5 to PC.";
        private const string Instruction3 = " - Open TeraTerm application.";
        private const string Instruction4 = " - Baudrate: 115200, DataBit 8, StopBit: One, Parity: None, ";
        private const string Instruction5 = "   Flow Control: None. ";
        private const string Instruction6 = " - Whatever you type on TeraTerm, SITCore will echo back the data + 1";
        private const string Instruction7 = " Example: Type 123... on TeraTerm, you will get back 234... ";
        private const string Instruction8 = " Press Test button when you are ready.";

        private readonly Button testButton;
        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;

        public UartWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
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
            if (!this.isRunning) {
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
