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

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;
        private Text instructionLabel4;
        private Text instructionLabel5;

        private string instruction1 = "This test will run on CAN1, FD mode enabled.";
        private string instruction2 = "Nominal speed: 1Mbit/s. Data speed: 2Mbit/s";
        private string instruction3 = "The test will send a message every 100ms,";
        private string instruction4 = "also receive the message id from 0x100...0x999.";
        private string instruction5 = "Press Test button when you ready.";

        private Text messageReceiveIdLabel;
        private Text messageReceiveFdModeLabel;
        private Text messageReceiveExtendedIdLabel;
        private Text messageReceiveRemoteTransmissionRequestLabel;
        private Text messageReceiveBitRateSwitchLabel;
        private Text messageReceiveDataLabel;
        private Text messageReceiveTotal;

        private Text messageSendIdLabel;
        private Text messageSendFdModeLabel;
        private Text messageSendExtendedIdLabel;
        private Text messageSendRemoteTransmissionRequestLabel;
        private Text messageSendBitRateSwitchLabel;
        private Text messageSendDataLabel;
        private Text messageSendTotal;

        private string arbitrationId = "ArbitrationId: ";
        private string extendedId = "ExtendedId: ";
        private string fdCan = "FD Mode: ";
        private string bitRateSwitch = "BitRateSwitch: ";
        private string remoteTransmissionRequest = "RTR: ";
        private string data = "Data: ";

        private Button testButton;

        private Font font;

        private bool isRunning;

        private int messageSentCount = 0;
        private int messageReceiveCount = 0;
        private CanMessage writeMsg;
        private CanMessage receiveMsg;

        public CanFdWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.instructionLabel1 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction1) {
                ForeColor = Colors.White,
            };

            this.instructionLabel2 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction2) {
                ForeColor = Colors.White,
            };

            this.instructionLabel3 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction3) {
                ForeColor = Colors.White,
            };

            this.instructionLabel4 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction4) {
                ForeColor = Colors.White,
            };

            this.instructionLabel5 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction5) {
                ForeColor = Colors.White,
            };

            this.messageSendIdLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageSendFdModeLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageSendExtendedIdLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageSendRemoteTransmissionRequestLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageSendBitRateSwitchLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageSendDataLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageSendTotal = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "0") {
                ForeColor = Colors.White,
            };

            // Receive
            this.messageReceiveIdLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageReceiveFdModeLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageReceiveExtendedIdLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageReceiveRemoteTransmissionRequestLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageReceiveBitRateSwitchLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageReceiveDataLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.messageReceiveTotal = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "0") {
                ForeColor = Colors.White,
            };

            this.testButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Test CanFd") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {

                this.UpdateResult(null, null);
                if (!this.isRunning)
                    new Thread(this.ThreadTestCanFd).Start();
            }
        }

        protected override void Active() {
            // To initialize, reset your variable, design...
            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.isRunning = false;

            this.messageSentCount = 0;
            this.messageReceiveCount = 0;
            this.writeMsg = null;
            this.receiveMsg = null;

            this.ClearScreen();
            this.CreateWindow();
        }

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() {
            this.isRunning = false;

            Thread.Sleep(10);
            // To stop or free, uinitialize variable resource
            this.canvas.Children.Clear();
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
                this.OnBottomBarButtonBackTouchUpEvent += this.TemplateWindow_OnBottomBarButtonBackTouchUpEvent;
                this.OnBottomBarButtonNextTouchUpEvent += this.TemplateWindow_OnBottomBarButtonNextTouchUpEvent;
            }

        }

        private void CreateWindow() {
            var startX = 20;
            var startY = 40;
            var offsetY = 30;

            Canvas.SetLeft(this.instructionLabel1, startX); Canvas.SetTop(this.instructionLabel1, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel1);

            Canvas.SetLeft(this.instructionLabel2, startX); Canvas.SetTop(this.instructionLabel2, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel2);


            Canvas.SetLeft(this.instructionLabel3, startX); Canvas.SetTop(this.instructionLabel3, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel3);

            Canvas.SetLeft(this.instructionLabel4, startX); Canvas.SetTop(this.instructionLabel4, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel4);

            Canvas.SetLeft(this.instructionLabel5, startX); Canvas.SetTop(this.instructionLabel5, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel5);


            Canvas.SetLeft(this.testButton, startX); Canvas.SetTop(this.testButton, startY); startY += offsetY;
            this.canvas.Children.Add(this.testButton);
        }

       

        private void UpdateResult(CanMessage write, CanMessage receive) {
            var startX1 = 20;
            var startY1 = 40;

            var startX2 = 240;
            var startY2 = 40;
            var offsetY = 30;

            this.ClearScreen();

            if (write != null) {
                this.writeMsg = write;
            }

            if (receive != null) {
                this.receiveMsg = receive;
            }

            // Update sending message
            if (this.writeMsg != null) {
                this.messageSendIdLabel.TextContent = this.arbitrationId + ((this.writeMsg == null) ? "N/A" : "" + this.writeMsg.ArbitrationId);
                this.messageSendFdModeLabel.TextContent = this.fdCan + ((this.writeMsg == null) ? "N/A" : "" + this.writeMsg.FdCan);
                this.messageSendExtendedIdLabel.TextContent = this.extendedId + ((this.writeMsg == null) ? "N/A" : "" + this.writeMsg.ExtendedId);
                this.messageSendRemoteTransmissionRequestLabel.TextContent = ((this.writeMsg == null) ? "N/A" : "" + this.writeMsg.RemoteTransmissionRequest);
                this.messageSendBitRateSwitchLabel.TextContent = ((this.writeMsg == null) ? "N/A" : "" + this.bitRateSwitch + this.writeMsg.BitRateSwitch);
                this.messageSendDataLabel.TextContent = this.data;

                for (var i = 0; i < 8; i++) {
                    this.messageSendDataLabel.TextContent += this.writeMsg.Data[i] + " ";
                }

                Canvas.SetLeft(this.messageSendIdLabel, startX1); Canvas.SetTop(this.messageSendIdLabel, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendIdLabel);

                Canvas.SetLeft(this.messageSendExtendedIdLabel, startX1); Canvas.SetTop(this.messageSendExtendedIdLabel, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendExtendedIdLabel);

                Canvas.SetLeft(this.messageSendFdModeLabel, startX1); Canvas.SetTop(this.messageSendFdModeLabel, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendFdModeLabel);

                Canvas.SetLeft(this.messageSendRemoteTransmissionRequestLabel, startX1); Canvas.SetTop(this.messageSendRemoteTransmissionRequestLabel, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendRemoteTransmissionRequestLabel);

                Canvas.SetLeft(this.messageSendBitRateSwitchLabel, startX1); Canvas.SetTop(this.messageSendBitRateSwitchLabel, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendBitRateSwitchLabel);

                Canvas.SetLeft(this.messageSendDataLabel, startX1); Canvas.SetTop(this.messageSendDataLabel, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendDataLabel);

                this.messageSendTotal.TextContent = "Total sent: " + this.messageSentCount;
                Canvas.SetLeft(this.messageSendTotal, startX1); Canvas.SetTop(this.messageSendTotal, startY1); startY1 += offsetY;
                this.canvas.Children.Add(this.messageSendTotal);
            }

            // Update receive message
            if (this.receiveMsg != null) {
                this.messageReceiveIdLabel.TextContent = this.arbitrationId + ((this.receiveMsg == null) ? "N/A" : "" + this.receiveMsg.ArbitrationId);
                this.messageReceiveFdModeLabel.TextContent = this.fdCan + ((this.receiveMsg == null) ? "N/A" : "" + this.receiveMsg.FdCan);
                this.messageReceiveExtendedIdLabel.TextContent = this.extendedId + ((this.receiveMsg == null) ? "N/A" : "" + this.receiveMsg.ExtendedId);
                this.messageReceiveRemoteTransmissionRequestLabel.TextContent = ((this.receiveMsg == null) ? "N/A" : "" + this.remoteTransmissionRequest + this.receiveMsg.RemoteTransmissionRequest);
                this.messageReceiveBitRateSwitchLabel.TextContent = this.bitRateSwitch + ((this.receiveMsg == null) ? "N/A" : "" + this.receiveMsg.BitRateSwitch);
                this.messageReceiveDataLabel.TextContent = this.data;

                for (var i = 0; i < 8; i++) {
                    this.messageReceiveDataLabel.TextContent += this.receiveMsg.Data[i] + " ";
                }

                Canvas.SetLeft(this.messageReceiveIdLabel, startX2); Canvas.SetTop(this.messageReceiveIdLabel, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveIdLabel);

                Canvas.SetLeft(this.messageReceiveExtendedIdLabel, startX2); Canvas.SetTop(this.messageReceiveExtendedIdLabel, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveExtendedIdLabel);

                Canvas.SetLeft(this.messageReceiveFdModeLabel, startX2); Canvas.SetTop(this.messageReceiveFdModeLabel, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveFdModeLabel);

                Canvas.SetLeft(this.messageReceiveRemoteTransmissionRequestLabel, startX2); Canvas.SetTop(this.messageReceiveRemoteTransmissionRequestLabel, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveRemoteTransmissionRequestLabel);

                Canvas.SetLeft(this.messageReceiveBitRateSwitchLabel, startX2); Canvas.SetTop(this.messageReceiveBitRateSwitchLabel, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveBitRateSwitchLabel);

                Canvas.SetLeft(this.messageReceiveDataLabel, startX2); Canvas.SetTop(this.messageReceiveDataLabel, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveDataLabel);

                this.messageReceiveTotal.TextContent = "Total Received: " + this.messageReceiveCount;
                Canvas.SetLeft(this.messageReceiveTotal, startX2); Canvas.SetTop(this.messageReceiveTotal, startY2); startY2 += offsetY;
                this.canvas.Children.Add(this.messageReceiveTotal);
            }

        }

        private void ThreadTestCanFd() {
            var canController = CanController.FromName(SC20260.CanBus.Can1);

            canController.SetNominalBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(13, 2, 3, 1, false)); // 1.0Mb at 48MHz

            //canController.SetNominalBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(15 + 8, 8, 6, 8, false)); // 250Kb at 48MHz

            canController.SetDataBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(8, 3, 2, 1, false)); // 2.0Mb at 48MHz

            canController.Filter.AddRangeFilter(Filter.IdType.Standard, 0x100, 0x7FF);
            canController.Filter.AddRangeFilter(Filter.IdType.Extended, 0x100, 0x999);


            canController.MessageReceived += this.CanController_MessageReceived;
            canController.ErrorReceived += this.CanController_ErrorReceived;

            canController.Enable();

            this.isRunning = true;

            var sendMsg = new CanMessage();

            while (this.isRunning) {
                while (canController.CanWriteMessage == false) ;

                sendMsg.ArbitrationId = 0x100;
                sendMsg.Data = new byte[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
                sendMsg.ExtendedId = true;
                sendMsg.Length = sendMsg.Data.Length;

                try {
                    canController.WriteMessage(sendMsg);
                }
                catch {
                    Thread.Sleep(1000);
                }

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1000), _ => {

                    this.UpdateResult(sendMsg, null);


                    return null;

                }, null);

                this.messageSentCount++;

                Thread.Sleep(100);
            }

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

            sender.ReadMessages(msgs, 0, msgs.Length);

            for (var i = 0; i < msgs.Length; i++) {
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(10), _ => {

                    this.UpdateResult(null, msgs[i]);

                    return null;

                }, null);
            }

            this.messageReceiveCount += msgs.Length;
        }
    }
}
