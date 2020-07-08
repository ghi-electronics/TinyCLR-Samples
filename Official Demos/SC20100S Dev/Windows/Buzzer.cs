using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class BuzzerWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;
        private Text instructionLabel4;
        private Text instructionLabel5;


        private string instruction1 = "This test Buzzer:";
        private string instruction2 = " First sound at 500Hz";
        private string instruction3 = " Second sound at 1000";
        private string instruction4 = " Third sound at 2000Hz";
        private string instruction5 = "Press Test button when you ready.";        

        private Font font;

        private bool isRuning;

        public object PwmController { get; private set; }

        public BuzzerWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
           
         
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);

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

        }

        private void Deinitialize() {
         

            this.instructionLabel1 = null;
            this.instructionLabel2 = null;
            this.instructionLabel3 = null;
            this.instructionLabel4 = null;
            this.instructionLabel5 = null;
     

            this.font.Dispose();

        }

        protected override void Active() {
            // To initialize, reset your variable, design...
            this.Initialize();

            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.ClearScreen();
            this.CreateWindow();
        }

        protected override void Deactive() {
            // To stop or free, uinitialize variable resource
            this.Deinitialize();
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
        }


        private void ThreadTest() {

            this.isRuning = true;

            var startX = 5;
            var startY = 20;
            var offsetY = 10;

            using (var pwmController3 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20260.PwmChannel.Controller3.Id)) {

                var pwmPinPB1 = pwmController3.OpenChannel(SC20260.PwmChannel.Controller3.PB1);

                pwmController3.SetDesiredFrequency(500);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.UpdateStatusText("Generate Pwm 500Hz...", startX, startY, true); startY += offsetY;

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                this.UpdateStatusText("Generate Pwm 1000Hz...", startX, startY, false); startY += offsetY;

                pwmController3.SetDesiredFrequency(1000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                this.UpdateStatusText("Generate Pwm 2000Hz...", startX, startY, false); startY += offsetY;

                pwmController3.SetDesiredFrequency(2000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Testing is success if you heard three", startX, startY, false); startY += offsetY;
                this.UpdateStatusText("kind of sounds!", startX, startY, false); startY += offsetY;
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
