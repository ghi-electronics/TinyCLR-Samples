using System.Drawing;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // CanFdContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same CAN FD test logic as before.
    public class CanFdWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = "This test will run on CAN1, FD mode.";
        private const string Instruction2 = " Nominal speed: 250Kbit/s.";
        private const string Instruction3 = " Data speed: 500Kbit/s.";
        private const string Instruction4 = " Filter Id: 0x100...0x999.";
        private const string Instruction5 = " When the board gets a message, it'll send back a message in the same format";
        private const string Instruction6 = " with ArbitrationId + 1.";
        private const string Instruction7 = " ";
        private const string Instruction8 = " Press Test button when you are ready.";

        private const string WaitForMessage = "Wait for receiving message...";
        private const string TotalReceived = "Total received: ";

        private const string ArbitrationId = "ArbitrationId: ";
        private const string ExtendedId = "ExtendedId: ";
        private const string FdCanMode = "FD Mode: ";
        private const string BitRateSwitch = "BitRateSwitch: ";
        private const string RTR = "RTR: ";
        private const string Data = "Data: ";

        private Button testButton;   // the designed TestButton, grabbed from the fragment
        private readonly Font font;
        private bool isRunning;
        private int messageReceiveCount;
        private TextFlow textFlow;    // the designed StatusText, grabbed from the fragment

        public CanFdWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);
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
                this.testButton.Visibility = Visibility.Collapsed; // hide while running (original removed it)
                this.textFlow.TextRuns.Clear();
                this.textFlow.TextRuns.Add(WaitForMessage, this.font, Colors.White);
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);
                new Thread(this.ThreadTest).Start();
            }
        }

        protected override void Active() {
            var content = new CanFdContent();
            this.canvas = content;
            this.textFlow = content.StatusText;
            this.testButton = content.TestButton;
            this.testButton.Click += this.TestButton_Click;
            this.testButton.Visibility = Visibility.Visible;
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

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
            this.Deinitialize();
        }

        private void ThreadTest() {
            this.isRunning = true;

            var canController = CanController.FromName(SC20260.CanBus.Can1);

            // 250 Kbit/s nominal.
            canController.SetNominalBitTiming(new CanBitTiming(15 + 8, 8, 6, 8, false));
            // 500 Kbit/s data.
            canController.SetDataBitTiming(new CanBitTiming(15 + 8, 8, 3, 8, false));

            canController.Filter.AddRangeFilter(Filter.IdType.Standard, 0x100, 0x7FF);
            canController.Filter.AddRangeFilter(Filter.IdType.Extended, 0x100, 0x999);

            canController.MessageReceived += this.CanController_MessageReceived;
            canController.ErrorReceived += this.CanController_ErrorReceived;

            canController.Enable();

            while (this.isRunning) {
                Thread.Sleep(100);
            }

            this.isRunning = false;
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
                for (var d = 0; d < 8; d++) {
                    dataText += msgs[i].Data[d] + " ";
                }
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
