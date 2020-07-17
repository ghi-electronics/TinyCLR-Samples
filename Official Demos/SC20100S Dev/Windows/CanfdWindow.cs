using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class CanFdWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This test will run on CAN1, FD mode.";
        private const string Instruction2 = " Nominal speed: 250Kbit/s.";
        private const string Instruction3 = " Data speed: 500Kbit/s.,";
        private const string Instruction4 = " Filter Id: 0x100...0x999.";
        private const string Instruction5 = " When the board get a message, it'll ";
        private const string Instruction6 = " send back a message same format";
        private const string Instruction7 = " and ArbitrationId plus 1. ";
        private const string Instruction8 = " Press Test button when you ready.";

        private const string WaitForMessage = "Wait for receiving message...";
        private const string TotalReceived = "Total received: ";

        private const string ArbitrationId = "ArbitrationId: ";
        private const string ExtendedId = "ExtendedId: ";
        private const string FdCanMode = "FD Mode: ";
        private const string BitRateSwitch = "BitRateSwitch: ";
        private const string RTR = "RTR: ";
        private const string Data = "Data: ";

        private Font font;

        private bool isRuning;

        private int messageReceiveCount = 0;

        private TextFlow textFlow;


        public CanFdWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {


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

            this.isRuning = false;

            this.messageReceiveCount = 0;

            this.ClearScreen();
            this.CreateWindow();

            this.SetEnableButtonNext(true);
        }

        protected override void Deactive() {
            this.isRuning = false;

            Thread.Sleep(10);

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
                        this.textFlow.TextRuns.Clear();

                        this.textFlow.TextRuns.Add(WaitForMessage, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                        this.textFlow.TextRuns.Add(TextRun.EndOfLine);

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

            var canController = CanController.FromName(SC20260.CanBus.Can1);

            canController.SetNominalBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(15 + 8, 8, 6, 8, false)); // 250Kbit/s          

            canController.SetDataBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(15 + 8, 8, 3, 8, false)); //500kbit/s 

            canController.Filter.AddRangeFilter(Filter.IdType.Standard, 0x100, 0x7FF);
            canController.Filter.AddRangeFilter(Filter.IdType.Extended, 0x100, 0x999);

            canController.MessageReceived += this.CanController_MessageReceived;
            canController.ErrorReceived += this.CanController_ErrorReceived;

            canController.Enable();

            while (this.isRuning) {

                Thread.Sleep(100);
            }

            this.isRuning = false;

            canController.Disable();
        }

        private void CanController_ErrorReceived(CanController sender, ErrorReceivedEventArgs e) {
            try {
                // Reset CAN
                sender.Disable();

                Thread.Sleep(10);

                sender.Enable();
            }
            catch { }
        }

        private void CanController_MessageReceived(CanController sender, MessageReceivedEventArgs e) {

            var msgs = new GHIElectronics.TinyCLR.Devices.Can.CanMessage[e.Count];

            for (var i = 0; i < msgs.Length; i++)
                msgs[i] = new GHIElectronics.TinyCLR.Devices.Can.CanMessage();

            this.messageReceiveCount += sender.ReadMessages(msgs, 0, msgs.Length);

            for (var i = 0; i < msgs.Length; i++) {
                this.UpdateStatusText(ArbitrationId + msgs[i].ArbitrationId, true);
                this.UpdateStatusText(FdCanMode + msgs[i].FdCan, false);
                this.UpdateStatusText(ExtendedId + msgs[i].ExtendedId, false);
                this.UpdateStatusText(RTR + msgs[i].RemoteTransmissionRequest, false);
                this.UpdateStatusText(BitRateSwitch + msgs[i].BitRateSwitch, false);

                var dataText = string.Empty;

                for (var ii = 0; ii < 8; ii++) {
                    dataText += msgs[i].Data[ii] + " ";
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

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
