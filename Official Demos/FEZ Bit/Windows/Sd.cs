using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class SdWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string FreeSize = "Total Free: ";
        private const string TotalSize = "Total Size: ";
        private const string VolumeLabel = "VolumeLabel: ";
        private const string RootDirectory = "RootDirectory: ";
        private const string DriveFormat = "DriveFormat: ";

        private const string Instruction1 = "This test will write 1K of data ";
        private const string Instruction2 = "to the file TEST_SD.TXT, then ";
        private const string Instruction3 = "read back to compare data.";
        private const string Instruction4 = " ";
        private const string Instruction5 = "Insert microSd and press Next";
        private const string Instruction6 = "button when you ready.";

        private const string BadConnect1 = "Bad device or no connect.";
        private const string BadConnect2 = "Data corrupted.";
        private const string BadWrite = "Write failed.";
        private const string BadRead = "Read failed.";

        private const string MountSuccess = "Mounted successful.";
        private const string TestSuccess = "Tested Read / Write successful.";

        private Font font;
        private TextFlow textFlow;
        private bool isRuning;

        public SdWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {


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


        protected override void Deactive() =>
            // To stop or free, uinitialize variable resource
            this.Deinitialize();

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
            const int BlockSize = 1024;

            this.isRuning = true;

            var data = System.Text.Encoding.UTF8.GetBytes("Thi is for sd  \n");

            var dataWrite = new byte[BlockSize];
            var dataRead = new byte[BlockSize];

            for (var i = 0; i < BlockSize; i += data.Length) {
                Array.Copy(data, 0, dataWrite, i, data.Length);
            }

            var storageController = StorageController.FromName(SC20260.StorageController.SdCard);

            IDriveProvider drive;


            try {
                drive = FileSystem.Mount(storageController.Hdc);

                var driveInfo = new DriveInfo(drive.Name);

                this.UpdateStatusText(FreeSize + driveInfo.TotalFreeSpace, true);
                this.UpdateStatusText(TotalSize + driveInfo.TotalSize, false);
                this.UpdateStatusText(VolumeLabel + driveInfo.VolumeLabel, false);
                this.UpdateStatusText(RootDirectory + driveInfo.RootDirectory, false);
                this.UpdateStatusText(DriveFormat + driveInfo.DriveFormat, false);
                this.UpdateStatusText(MountSuccess, false);

            }
            catch {

                this.UpdateStatusText(BadConnect1, true);

                goto _return;
            }

            Thread.Sleep(1000);

            var filename = drive.Name + "\\TEST_SD.TXT";

            try {
                using (var fsWrite = new FileStream(filename, FileMode.Create)) {

                    fsWrite.Write(dataWrite, 0, dataWrite.Length);

                    fsWrite.Flush();
                    fsWrite.Close();
                }
            }
            catch {
                this.UpdateStatusText(BadWrite, false);

                goto _unmount;
            }

            try {
                using (var fsRead = new FileStream(filename, FileMode.Open)) {

                    fsRead.Read(dataRead, 0, dataRead.Length);

                    for (var i = 0; i < dataRead.Length; i++) {


                        if (dataRead[i] != dataWrite[i]) {
                            this.UpdateStatusText(BadConnect2, false);

                            goto _unmount;
                        }
                    }

                    fsRead.Flush();
                    fsRead.Close();
                }
            }
            catch {
                this.UpdateStatusText(BadRead, false);

                goto _unmount;
            }


            this.UpdateStatusText(TestSuccess, false);

_unmount:
            try {
                GHIElectronics.TinyCLR.IO.FileSystem.Flush(storageController.Hdc);
                GHIElectronics.TinyCLR.IO.FileSystem.Unmount(storageController.Hdc);
            }
            catch {
            }

_return:

            this.isRuning = false;

        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
