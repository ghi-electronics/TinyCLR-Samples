using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Drivers.Omnivision.Ov9655;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class ColorWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;
        private Text instructionLabel4;
        private Text instructionLabel5;
        private Text instructionLabel6;
        private Text instructionLabel7;
        private Text instructionLabel8;


        private string instruction1 = " This will test raw graphic on UD435 display: ";
        private string instruction2 = " ";
        private string instruction3 = " ";
        private string instruction4 = "  Press Test button when you ready.";
        private string instruction5 = "  ";
        private string instruction6 = "  ";
        private string instruction7 = "  ";

        private string instruction8 = " ";

        private Button testButton;

        private Font font;

        private bool isRuning;

        public object PwmController { get; private set; }

        public ColorWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
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


            var displayController = Display.DisplayController;

            var screen = System.Drawing.Graphics.FromHdc(displayController.Hdc);

            var background = Resources.GetBitmap(Resources.BitmapResources.Color);

            var startY = 0;
            var imageX = 0;

            var screen_width = 480;
            var screen_height = 272 - this.TopBar.ActualHeight;

            while (this.isRuning) {

                screen.Clear();

                screen.DrawImage(background, imageX, startY);

                screen.FillEllipse(new SolidBrush(System.Drawing.Color.FromArgb(100, 0xFF, 0, 0)), 0, startY, 100, 100);


                screen.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0, 0xFF)), 0, 120, screen_width / 4, screen_height - 120);
                screen.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0xFF, 0)), screen_width / 4, 120, screen_width / 4, screen_height - 120);
                screen.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(100, 0xFF, 0, 0)), screen_width / 4 * 2, 120, screen_width / 4, screen_height - 120);
                screen.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(100, 0xFF, 0xFF, 0xFF)), screen_width / 4 * 3, 120, screen_width / 4, screen_height - 120);


                screen.DrawString("This is blue", this.font, new SolidBrush(System.Drawing.Color.Blue), 0, screen_height - 140);
                screen.DrawString("This is green", this.font, new SolidBrush(System.Drawing.Color.Green), screen_width / 4, screen_height - 140);
                screen.DrawString("This is red", this.font, new SolidBrush(System.Drawing.Color.Red), screen_width / 4 * 2, screen_height - 140);
                screen.DrawString("This is white", this.font, new SolidBrush(System.Drawing.Color.White), screen_width / 4 * 3, screen_height - 140);
                screen.DrawString("This is transparent", this.font, new SolidBrush(System.Drawing.Color.White), 20, 20);
                screen.DrawString("Touch here to return main menu [X]", this.font, new SolidBrush(System.Drawing.Color.White), screen_width / 2, 5);




                screen.Flush();

                GC.Collect();
                GC.WaitForPendingFinalizers();
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