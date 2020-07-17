using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;

using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class UartWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This will test UART1 only: ";
        private const string Instruction2 = "- Connect UART1 to PC.";
        private const string Instruction3 = "- Open TeraTerm application.";
        private const string Instruction4 = "- Baud: 115200, DataBit 8, StopBit: 1,";
        private const string Instruction5 = "  Parity: None, Flow Control: None. ";
        private const string Instruction6 = "- Whatever you typed on TeraTerm, ";
        private const string Instruction7 = "SITCore will back these data +1 ";
        private const string Instruction8 = " ";

        private const string WaitForMessage = "Wait for receiving data...";

        private Font font;

        private bool isRuning;

        private TextFlow textFlow;

        public UartWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {

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

            Thread.Sleep(100); // Wait for test thread is stop => no update canvas

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

                        this.SetEnableButtonNext(false);

                        new Thread(this.ThreadTest).Start();
                    }
                    break;
            }
        }

        private void CreateWindow() {
            var startX = 2;
            var startY = 20;

            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);
        }


        private void ThreadTest() {

            this.isRuning = true;


            using (var uart1 = UartController.FromName(SC20260.UartPort.Uart5)) {

                var setting = new UartSetting() {
                    BaudRate = 115200
                };

                uart1.SetActiveSettings(setting);
                uart1.Enable();

                var totalReceived = 0;
                var totalSent = 0;

                while (this.isRuning) {
                    this.UpdateStatusText("Total received: " + totalReceived, true);
                    this.UpdateStatusText("Total sent: " + totalSent, false);
                    this.UpdateStatusText(WaitForMessage, false);

                    while (uart1.BytesToRead == 0) {
                        Thread.Sleep(10);
                    }

                    var byteToRead = uart1.BytesToRead > uart1.ReadBufferSize ? uart1.ReadBufferSize : uart1.BytesToRead;

                    var read = new byte[byteToRead];


                    this.UpdateStatusText("Receiving... " + byteToRead + " byte(s)", false);
                    totalReceived += uart1.Read(read);



                    for (var i = 0; i < read.Length; i++) {
                        var write = new byte[1] { (byte)(read[i] + 1) };
                        totalSent += uart1.Write(write);

                        uart1.Flush();
                    }


                    this.UpdateStatusText("Writing back... " + byteToRead + " byte(s)", false);

                }
            }


            this.isRuning = false;

            return;

        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
