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

        private Text freeSizeLabel;
        private Text totalSizeLabel;
        private Text volumeLabel;
        private Text rootDirectoryLabel;
        private Text driverFormatLabel;
        private Text statusLabel;

        private Text instructionLabel1;
        private Text instructionLabel2;
        private Text instructionLabel3;
        private Text instructionLabel4;
        private Text instructionLabel5;
        private Text instructionLabel6;

        private string freeSize = "Total Free: ";
        private string totalSize = "Total Size: ";
        private string volume = "VolumeLabel: ";
        private string rootDirectory = "RootDirectory: ";
        private string driverFormat = "DriveFormat: ";

        private string instruction1 = "This test will write 1K of data ";
        private string instruction2 = "to the file TEST_SD.TXT, then ";
        private string instruction3 = "read back to compare data.";
        private string instruction4 = " ";
        private string instruction5 = "Insert microSd and press Next";
        private string instruction6 = "button when you ready.";

        private Font font;

        public SdWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {


        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);

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

            this.instructionLabel4 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction4) {
                ForeColor = Colors.White,
            };

            this.instructionLabel5 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction5) {
                ForeColor = Colors.White,
            };

            this.instructionLabel6 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.instruction6) {
                ForeColor = Colors.White,
            };
        }

        private void Deinitialize() {
            this.freeSizeLabel = null;
            this.totalSizeLabel = null;
            this.volumeLabel = null;
            this.rootDirectoryLabel = null;
            this.driverFormatLabel = null;
            this.statusLabel = null;

            this.instructionLabel1 = null;
            this.instructionLabel2 = null;
            this.instructionLabel3 = null;
            this.instructionLabel4 = null;
            this.instructionLabel5 = null;
            this.instructionLabel6 = null;

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

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() {
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

            Canvas.SetLeft(this.instructionLabel3, startX); Canvas.SetTop(this.instructionLabel6, startY); startY += offsetY;
            this.canvas.Children.Add(this.instructionLabel6);
        }

        private string testStatus;
        private bool isRuning;

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

            var startX = 5;
            var startY = 20;
            var offsetY = 10;

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

            this.isRuning = false;

        }
    }
}
