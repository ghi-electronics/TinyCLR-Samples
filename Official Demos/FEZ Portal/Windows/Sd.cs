using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
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

        private const string Instruction1 = "This test will write 1K of data to the file TEST_SD.TXT,";
        private const string Instruction2 = "then read back to compare data.";
        private const string Instruction3 = "Insert microSd and press Test Button when you are ready.";

        private const string BadConnect1 = "Bad device or no connect.";
        private const string BadConnect2 = "Data corrupted.";
        private const string BadWrite = "Write failed.";
        private const string BadRead = "Read failed.";

        private const string MountSuccess = "Mounted successful.";
        private const string TestSuccess = "Tested Read / Write successful.";

        private Button testButton;

        private Font font;

        private bool isRunning;

        private TextFlow textFlow;

        public SdWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.testButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Test Sd") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;
        }

        private void Initialize() {

            this.textFlow = new TextFlow();

            this.textFlow.TextRuns.Add(Instruction1, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction2, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction3, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

        }

        private void Deinitialize() {

            this.textFlow.TextRuns.Clear();
            this.textFlow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {

                if (!this.isRunning) {
                    this.ClearScreen();

                    this.CreateWindow(false);

                    this.textFlow.TextRuns.Clear();

                    new Thread(this.ThreadTest).Start();
                }
            }
        }


        protected override void Active() {
            // To initialize, reset your variable, design...
            this.Initialize();

            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.isRunning = false;

            this.ClearScreen();
            this.CreateWindow(true);
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
                this.OnBottomBarButtonBackTouchUpEvent += this.TemplateWindow_OnBottomBarButtonBackTouchUpEvent;
                this.OnBottomBarButtonNextTouchUpEvent += this.TemplateWindow_OnBottomBarButtonNextTouchUpEvent;
            }

        }

        private void CreateWindow(bool enablebutton) {
            var startX = 5;
            var startY = 40;

            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);

            if (enablebutton) {
                var buttonY = this.Height - ((this.testButton.Height * 3) / 2);

                Canvas.SetLeft(this.testButton, startX); Canvas.SetTop(this.testButton, buttonY);
                this.canvas.Children.Add(this.testButton);
            }
        }


        private void ThreadTest() {
            const int BlockSize = 1024;

            this.isRunning = true;

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

            this.isRunning = false;

        }

        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);
    }
}
