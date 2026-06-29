using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class UartWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;

        private const string Instruction1 = "This will test UART1 only:";
        private const string Instruction2 = "- Connect UART1 to PC.";
        private const string Instruction3 = "- Open TeraTerm application.";
        private const string Instruction4 = "- Baud: 115200, Data 8, Stop 1,";
        private const string Instruction5 = "  Parity: None, FlowCtrl: None.";
        private const string Instruction6 = "- Whatever you type on TeraTerm,";
        private const string Instruction7 = "  SITCore echoes it back + 1.";

        private const string WaitForMessage = "Wait for receiving data...";

        public UartWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
            this.AppendInstruction(Instruction6);
            this.AppendInstruction(Instruction7);
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
            Canvas.SetLeft(this.textFlow, 2);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }

        private void ThreadTest() {
            this.isRunning = true;

            using (var uart1 = UartController.FromName(SC20100.UartPort.Uart1)) {
                uart1.SetActiveSettings(new UartSetting { BaudRate = 115200 });
                uart1.Enable();

                var totalReceived = 0;
                var totalSent = 0;

                while (this.isRunning) {
                    this.UpdateStatusText("Total received: " + totalReceived, true);
                    this.UpdateStatusText("Total sent: " + totalSent, false);
                    this.UpdateStatusText(WaitForMessage, false);

                    while (uart1.BytesToRead == 0)
                        Thread.Sleep(10);

                    var byteToRead = uart1.BytesToRead > uart1.ReadBufferSize ? uart1.ReadBufferSize : uart1.BytesToRead;
                    var read = new byte[byteToRead];

                    this.UpdateStatusText("Receiving... " + byteToRead + " byte(s)", false);
                    totalReceived += uart1.Read(read);

                    for (var i = 0; i < read.Length; i++) {
                        totalSent += uart1.Write(new byte[] { (byte)(read[i] + 1) });
                        uart1.Flush();
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
