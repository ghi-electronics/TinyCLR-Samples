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
        private const string Instruction2 = " Nominal speed: 1Mbit/s.";
        private const string Instruction3 = " Data speed: 2Mbit/.,";
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
              
            this.textFlow.TextRuns.Clear();
            this.textFlow = null;

            this.font.Dispose();

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

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() {
            this.isRuning = false;

            Thread.Sleep(10);
            // To stop or free, uinitialize variable resource
            this.canvas.Children.Clear();

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
                    if (this.isRuning == false) {
                        this.textFlow.TextRuns.Clear();

                        this.textFlow.TextRuns.Add(WaitForMessage, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                        this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                        this.SetEnableButtonNext(false);

                        new Thread(this.ThreadTest).Start();
                    }
                    break;

                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select:

                    break;

            }
        }

        private void CreateWindow() {
            var startX = 5;
            var startY = 20;
            var offsetY = 10;

          
            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY); startY += offsetY;
            this.canvas.Children.Add(this.textFlow);
        }


        private void UpdateResult(CanMessage write, CanMessage receiveMsg) {

            this.textFlow.TextRuns.Clear();

            // Update receive message
            if (receiveMsg != null) {
                

                this.textFlow.TextRuns.Add(ArbitrationId + receiveMsg.ArbitrationId, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                this.textFlow.TextRuns.Add(FdCanMode + receiveMsg.FdCan, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                this.textFlow.TextRuns.Add(ExtendedId + receiveMsg.ExtendedId, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                this.textFlow.TextRuns.Add(RTR + receiveMsg.RemoteTransmissionRequest, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                this.textFlow.TextRuns.Add(BitRateSwitch + receiveMsg.BitRateSwitch, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                var dataText = string.Empty;

                for (var i = 0; i < 8; i++) {
                    dataText += receiveMsg.Data[i] + " ";
                }

                this.textFlow.TextRuns.Add(Data + dataText, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                this.textFlow.TextRuns.Add(TotalReceived + this.messageReceiveCount, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
                this.textFlow.TextRuns.Add(TextRun.EndOfLine);

               
               
            }
            else {

            }           
        }

        private void ThreadTest() {
            this.isRuning = true;

            var canController = CanController.FromName(SC20260.CanBus.Can1);

            canController.SetNominalBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(13, 2, 3, 1, false)); // 1.0Mb at 48MHz            

            canController.SetDataBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(8, 3, 2, 1, false)); // 2.0Mb at 48MHz

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
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {

                    this.UpdateResult(null, msgs[i]);

                    return null;

                }, null);

                try {
                    msgs[i].ArbitrationId += 1;

                    sender.WriteMessage(msgs[i]);
                }
                catch {

                }
            }            
        }        
    }
}
