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

        private Text freeSizeLabel;
        private Text totalSizeLabel;
        private Text volumeLabel;
        private Text rootDirectoryLabel;
        private Text driverFormatLabel;
        private Text statusLabel;

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;

        private string freeSize = "Total Free: ";
        private string totalSize = "Total Size: ";
        private string volume = "VolumeLabel: ";
        private string rootDirectory = "RootDirectory: ";
        private string driverFormat = "DriveFormat: ";

        private string instruction1 = "This test will write 1K of data to the file TEST_SD.TXT,";
        private string instruction2 = "then read back to compare data.";
        private string instruction3 = "Insert microSd and press Test Button when you ready.";

        private Button testButton;

        private Font font;

        public SdWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.freeSizeLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.freeSize) {
                ForeColor = Colors.White,
            };

            this.totalSizeLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.totalSize) {
                ForeColor = Colors.White,
            };

            this.volumeLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.volume) {
                ForeColor = Colors.White,
            };

            this.rootDirectoryLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.rootDirectory) {
                ForeColor = Colors.White,
            };

            this.driverFormatLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.driverFormat) {
                ForeColor = Colors.White,
            };

            this.statusLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "") {
                ForeColor = Colors.White,
            };

            this.instructionLabel1 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction1) {
                ForeColor = Colors.White,
            };

            this.instructionLabel2 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction2) {
                ForeColor = Colors.White,
            };

            this.instructionLabel3 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction3) {
                ForeColor = Colors.White,
            };

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

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                if (!this.isRunning)
                    new Thread(this.ThreadTestSd).Start();

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


            Canvas.SetLeft(this.testButton, startX); Canvas.SetTop(this.testButton, startY); startY += offsetY;
            this.canvas.Children.Add(this.testButton);
        }

        private string testStatus;
        private bool isRunning;

        private void ThreadTestSd() {
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

            var startX = 20;
            var startY = 40;
            var offsetY = 30;



            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {
                this.ClearScreen();
                this.freeSizeLabel.TextContent = this.freeSize;
                this.totalSizeLabel.TextContent = this.totalSize;
                this.volumeLabel.TextContent = this.volume;
                this.rootDirectoryLabel.TextContent = this.rootDirectory;
                this.driverFormatLabel.TextContent = this.driverFormat;
                this.statusLabel.TextContent = string.Empty;

                Canvas.SetLeft(this.freeSizeLabel, startX); Canvas.SetTop(this.freeSizeLabel, startY); startY += offsetY;
                this.canvas.Children.Add(this.freeSizeLabel);

                Canvas.SetLeft(this.totalSizeLabel, startX); Canvas.SetTop(this.totalSizeLabel, startY); startY += offsetY;
                this.canvas.Children.Add(this.totalSizeLabel);

                Canvas.SetLeft(this.volumeLabel, startX); Canvas.SetTop(this.volumeLabel, startY); startY += offsetY;
                this.canvas.Children.Add(this.volumeLabel);

                Canvas.SetLeft(this.rootDirectoryLabel, startX); Canvas.SetTop(this.rootDirectoryLabel, startY); startY += offsetY;
                this.canvas.Children.Add(this.rootDirectoryLabel);

                Canvas.SetLeft(this.driverFormatLabel, startX); Canvas.SetTop(this.driverFormatLabel, startY); startY += offsetY;
                this.canvas.Children.Add(this.driverFormatLabel);

                Canvas.SetLeft(this.statusLabel, startX); Canvas.SetTop(this.statusLabel, startY); startY += offsetY;
                this.canvas.Children.Add(this.statusLabel);

                return null;

            }, null);

            Thread.Sleep(1000);

            try {
                drive = FileSystem.Mount(storageController.Hdc);

                var driveInfo = new DriveInfo(drive.Name);

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                    this.freeSizeLabel.TextContent += driveInfo.TotalFreeSpace;
                    this.totalSizeLabel.TextContent += driveInfo.TotalSize;
                    this.volumeLabel.TextContent += driveInfo.VolumeLabel;
                    this.rootDirectoryLabel.TextContent += driveInfo.RootDirectory;
                    this.driverFormatLabel.TextContent += driveInfo.DriveFormat;
                    this.statusLabel.TextContent += "Mounted successfully.";


                    return null;

                }, null);
            }
            catch {
                this.testStatus = "Bad device or no device connected.";

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                    this.statusLabel.TextContent = this.testStatus;

                    this.statusLabel.Invalidate();
                    return null;

                }, null);

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
                this.testStatus = "Test write failed.";

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                    this.statusLabel.TextContent = this.testStatus;

                    this.statusLabel.Invalidate();
                    return null;

                }, null);

                goto _unmount;
            }

            try {
                using (var fsRead = new FileStream(filename, FileMode.Open)) {

                    fsRead.Read(dataRead, 0, dataRead.Length);

                    for (var i = 0; i < dataRead.Length; i++) {


                        if (dataRead[i] != dataWrite[i]) {
                            this.testStatus = "Test Read: Data corrupted.";

                            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                                this.statusLabel.TextContent = this.testStatus;

                                this.statusLabel.Invalidate();
                                return null;

                            }, null);

                            goto _unmount;
                        }
                    }

                    fsRead.Flush();
                    fsRead.Close();
                }
            }
            catch {
                this.testStatus = "Test write failed.";

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                    this.statusLabel.TextContent = this.testStatus;

                    this.statusLabel.Invalidate();
                    return null;

                }, null);

                goto _unmount;
            }

            this.testStatus = "Tested Write/Read successfully.";

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                this.statusLabel.TextContent = this.testStatus;

                this.statusLabel.Invalidate();
                return null;

            }, null);

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
    }
}
