using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class RtcWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;
        private Text instructionLabel4;
        private Text instructionLabel5;
        private Text instructionLabel6;
        private Text instructionLabel7;
        private Text instructionLabel8;


        private string instruction1 = " ***Be carefull: This test will enable charging mode on VBAT pin*** ";
        private string instruction2 = " This will test RTC. The time will start at 00:00:00 - 07/07/2020";
        private string instruction3 = " - Wait for charing about 30 seconds.";
        private string instruction4 = " - Power off the board for 10 seconds";
        private string instruction5 = " - Power on the board. ";
        private string instruction6 = " => Passed if timer is after 00:00:00 - 07/07/2020 ";
        private string instruction7 = "    Failed if timer is reset to 00:00:00 - 01/01/2017 ";
        private string instruction8 = " Press Test button when you ready.";

        private Button testButton;

        private Font font;

        private bool isRuning;

        private RtcController rtc;

        public object PwmController { get; private set; }

        const int ChargeVbatTimeout = 30; // 30 seconds

        public RtcWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

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

            this.instructionLabel6 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction6) {
                ForeColor = Colors.White,
            };

            this.instructionLabel7 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction7) {
                ForeColor = Colors.White,
            };

            this.instructionLabel8 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction8) {
                ForeColor = Colors.White,
            };

            this.rtc = RtcController.GetDefault();

create_button:

            try {
                this.testButton = new Button {
                    Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Start Test!") {
                        ForeColor = Colors.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },

                    Width = 100,
                    Height = 30
                };
            }
            catch {

            }

            if (this.testButton == null) {
                goto create_button;
            }

            this.testButton.Click += this.TestButton_Click;

            this.rtc.SetChargeMode(BatteryChargeMode.None);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                if (this.isRuning == false) {
                    new Thread(this.ThreadTest).Start();
                }
            }
        }

        protected override void Active() {
            // To initialize, reset your variable, design...
            this.canvas = new Canvas();

            this.Child = this.canvas;

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
            this.isRuning = false;

            Thread.Sleep(100); // Wait for test thread is stop => no update canvas
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
            var offsetY = 20;

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


            Canvas.SetLeft(this.instructionLabel6, startX); Canvas.SetTop(this.instructionLabel6, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel6);


            Canvas.SetLeft(this.instructionLabel7, startX); Canvas.SetTop(this.instructionLabel7, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel7);


            Canvas.SetLeft(this.instructionLabel8, startX); Canvas.SetTop(this.instructionLabel8, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel8);

            Canvas.SetLeft(this.testButton, startX); Canvas.SetTop(this.testButton, startY); startY += offsetY;
            this.canvas.Children.Add(this.testButton);
        }


        private void ThreadTest() {

            this.isRuning = true;

            var startX = 20;
            var startY = 40;
            var offsetY = 30;


            var m = new DateTime(2020, 7, 7, 00, 00, 00);

            if (this.rtc.IsValid && this.rtc.Now > m) {

                while (this.isRuning) {
                    var time = this.rtc.Now.ToString();

                    startX = 20;
                    startY = 40;

                    this.UpdateStatusText("RTC is working: " + time, startX, startY, true); startY += offsetY;
                }
            }
            else {


                var newDt = RtcDateTime.FromDateTime(m);

                var charetimeout = 0;

                this.rtc.SetChargeMode(BatteryChargeMode.Fast);

                while (charetimeout < ChargeVbatTimeout) {
                    this.UpdateStatusText("Please wait for charing...." + charetimeout + " / " + ChargeVbatTimeout, startX, startY, true);

                    charetimeout++;

                    Thread.Sleep(1000);
                }

                startY += offsetY;

                this.rtc.SetChargeMode(BatteryChargeMode.None);

                this.rtc.SetTime(newDt);
                GHIElectronics.TinyCLR.Native.SystemTime.SetTime(m);

                this.UpdateStatusText("Please power off the board for 10 seconds", startX, startY, false); startY += offsetY;
            }


            this.isRuning = false;

            return;

        }

        private void UpdateStatusText(string text, int x, int y, bool clearscreen) {

            var timeout = 10;

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(timeout), _ => {

                if (clearscreen)
                    this.ClearScreen();


                var label = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, text) {
                    ForeColor = Colors.White,
                };


                Canvas.SetLeft(label, x); Canvas.SetTop(label, y);
                this.canvas.Children.Add(label);

                label.Invalidate();

                return null;

            }, null);

            Thread.Sleep(timeout);

        }
    }
}
