using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class QspiWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;
        private Text instructionLabel4;
        private Text instructionLabel5;
        private Text statusLabel;

        private string instruction1 = "This test will Erase/Write/Read:";
        private string instruction2 = " - 8 frist sectors";
        private string instruction3 = " - 8 last sectors";
        private string instruction4 = "All exist data on these sectors will be erased!";
        private string instruction5 = "Press Test button when you ready.";

        private Button testButton;

        private Font font;

        private bool isRuning;

        public QspiWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
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

            this.statusLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, string.Empty) {
                ForeColor = Colors.White,
            };

            this.testButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Start Test!") {
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

        protected override void Deactive() =>
            // To stop or free, uinitialize variable resource
            this.canvas.Children.Clear();

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

        private string status = string.Empty;

        private void ThreadTest() {

            this.isRuning = true;
            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);

            var drive = storeController.Provider;

            drive.Open();


            var sectorSize = drive.Descriptor.RegionSizes[0];

            var textWrite = System.Text.UTF8Encoding.UTF8.GetBytes("this is for test");
            var dataRead = new byte[sectorSize];

            var dataWrite = new byte[sectorSize];

            for (var i = 0; i < sectorSize; i += textWrite.Length) {
                Array.Copy(textWrite, 0, dataWrite, i, textWrite.Length);

            }

            var roundTest = 0;
            var startSector = 0;
            var endSector = 8;

_again:
            if (roundTest == 1) {
                startSector = 4088; // last 8 sectors
                endSector = startSector + 8;
            }

            var startX = 20;
            var startY = 40;
            var offsetY = 30;

            for (var s = startSector; s < endSector; s++) {

                startX = 20;
                startY = 40;
                offsetY = 30;

                var address = s * sectorSize;
                this.UpdateStatusText("Erasing sector " + s, startX, startY, true); startY += offsetY;
                // Erase
                drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                // Read - check for blank
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != 0xFF) {

                        this.UpdateStatusText("Erase failed at: " + idx, startX, startY, false); startY += offsetY;

                        goto _return;
                    }
                }

                // Write
                this.UpdateStatusText("Writing sector " + s, startX, startY, false); startY += offsetY;
                drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                this.UpdateStatusText("Reading sector " + s, startX, startY, false); startY += offsetY;
                //Read to compare
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));


                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != dataWrite[idx]) {

                        this.UpdateStatusText("Compare failed at: " + idx, startX, startY, false); startY += offsetY;

                        goto _return;
                    }

                }
            }

            roundTest++;

            if (roundTest == 2) {
                this.UpdateStatusText("Tested Quad Spi successful!", startX, startY, false); startY += offsetY;
            }
            else {
                goto _again;
            }


_return:
            drive.Close();
            this.isRuning = false;

            return;

        }

        private void UpdateStatusText(string text, int x, int y, bool clearscreen) {

            Thread.Sleep(1);

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(10), _ => {

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

        }
    }
}
