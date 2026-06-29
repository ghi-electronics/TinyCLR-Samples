using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class CanFdWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;
        private int messageReceiveCount;

        private const string Instruction1 = "This test will run on CAN1, FD mode.";
        private const string Instruction2 = " Nominal speed: 250Kbit/s.";
        private const string Instruction3 = " Data speed: 500Kbit/s.";
        private const string Instruction4 = " Filter Id: 0x100...0x999.";
        private const string Instruction5 = " On any received message the board";
        private const string Instruction6 = " echoes it back with ArbitrationId+1.";
        private const string Instruction7 = " ";
        private const string Instruction8 = " Press Test when you are ready.";

        private const string WaitForMessage = "Wait for receiving message...";
        private const string TotalReceived  = "Total received: ";

        private const string ArbitrationId  = "ArbitrationId: ";
        private const string ExtendedId     = "ExtendedId: ";
        private const string FdCanMode      = "FD Mode: ";
        private const string BitRateSwitch  = "BitRateSwitch: ";
        private const string RTR            = "RTR: ";
        private const string Data           = "Data: ";

        public CanFdWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
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
            this.AppendInstruction(Instruction8);
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
            this.isRunning = false;
            this.messageReceiveCount = 0;
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
                    if (!this.isRunning) {
                        this.textFlow.TextRuns.Clear();
                        this.textFlow.TextRuns.Add(WaitForMessage, this.font, Colors.White);
                        this.textFlow.TextRuns.Add(TextRun.EndOfLine);
                        new Thread(this.ThreadTest).Start();
                    }
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

            var canController = CanController.FromName(SC20100.CanBus.Can1);
            canController.SetNominalBitTiming(new CanBitTiming(15 + 8, 8, 6, 8, false)); // 250 Kbit/s
            canController.SetDataBitTiming(new CanBitTiming(15 + 8, 8, 3, 8, false));    // 500 Kbit/s

            canController.Filter.AddRangeFilter(Filter.IdType.Standard, 0x100, 0x7FF);
            canController.Filter.AddRangeFilter(Filter.IdType.Extended, 0x100, 0x999);

            canController.MessageReceived += this.CanController_MessageReceived;
            canController.ErrorReceived += this.CanController_ErrorReceived;
            canController.Enable();

            while (this.isRunning)
                Thread.Sleep(100);

            canController.Disable();
        }

        private void CanController_ErrorReceived(CanController sender, ErrorReceivedEventArgs e) {
            try {
                sender.Disable();
                Thread.Sleep(10);
                sender.Enable();
            }
            catch {
            }
        }

        private void CanController_MessageReceived(CanController sender, MessageReceivedEventArgs e) {
            var msgs = new CanMessage[e.Count];
            for (var i = 0; i < msgs.Length; i++)
                msgs[i] = new CanMessage();

            this.messageReceiveCount += sender.ReadMessages(msgs, 0, msgs.Length);

            for (var i = 0; i < msgs.Length; i++) {
                this.UpdateStatusText(ArbitrationId + msgs[i].ArbitrationId, true);
                this.UpdateStatusText(FdCanMode + msgs[i].FdCan, false);
                this.UpdateStatusText(ExtendedId + msgs[i].ExtendedId, false);
                this.UpdateStatusText(RTR + msgs[i].RemoteTransmissionRequest, false);
                this.UpdateStatusText(BitRateSwitch + msgs[i].BitRateSwitch, false);

                var dataText = string.Empty;
                for (var d = 0; d < 8; d++)
                    dataText += msgs[i].Data[d] + " ";

                this.UpdateStatusText(Data + dataText, false);
                this.UpdateStatusText(TotalReceived + this.messageReceiveCount, false);

                try {
                    msgs[i].ArbitrationId += 1;
                    sender.WriteMessage(msgs[i]);
                }
                catch {
                }
            }
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
