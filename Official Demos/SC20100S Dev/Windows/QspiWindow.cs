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

        private const string Instruction1 = "This test will Erase/Write/Read:";
        private const string Instruction2 = " - 8 frist sectors";
        private const string Instruction3 = " - 8 last sectors";
        private const string Instruction4 = "All exist data on these sectors ";
        private const string Instruction5 = "will be erased!";


        private Font font;

        private bool isRuning;

        private TextFlow textFlow;

        public QspiWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {

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
        }

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() => this.Deinitialize();

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
            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);

            var drive = storeController.Provider;

            drive.Open();


            var sectorSize = drive.Descriptor.RegionSizes[0];

            var textWrite = System.Text.UTF8Encoding.UTF8.GetBytes("this is for test");

            GC.Collect();
            GC.WaitForPendingFinalizers();

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

            for (var s = startSector; s < endSector; s++) {
                var address = s * sectorSize;
                this.UpdateStatusText("Erasing sector " + s, true);

                // Erase
                drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                // Write
                this.UpdateStatusText("Writing sector " + s, false);
                drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                this.UpdateStatusText("Reading sector " + s, false);
                //Read to compare
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));


                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != dataWrite[idx]) {

                        this.UpdateStatusText("Compare failed at: " + idx, false);

                        goto _return;
                    }

                }
            }

            roundTest++;

            if (roundTest == 2) {
                this.UpdateStatusText("Tested Quad Spi successful!", false);
            }
            else {
                goto _again;
            }


_return:
            drive.Close();
            this.isRuning = false;

            return;

        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
