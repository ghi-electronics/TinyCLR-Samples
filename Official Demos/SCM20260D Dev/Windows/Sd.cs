using System;
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
using SystemDrawing = System.Drawing;

namespace Demos {
    public class SdWindow : ApplicationWindow {
        private Canvas canvas;

        private const string FreeSize = "Total Free: ";
        private const string TotalSize = "Total Size: ";
        private const string VolumeLabel = "VolumeLabel: ";
        private const string RootDirectory = "RootDirectory: ";
        private const string DriveFormat = "DriveFormat: ";

        private const string Instruction1 = "This test will write 1K of data to the file TEST_SD.TXT,";
        private const string Instruction2 = "then read back to compare data.";
        private const string Instruction3 = "Insert microSd and press Test Button when you are ready.";

        private const string BadConnect = "Bad device or no connect.";
        private const string DataCorrupted = "Data corrupted.";
        private const string BadWrite = "Write failed.";
        private const string BadRead = "Read failed.";

        private const string MountSuccess = "Mounted successful.";
        private const string TestSuccess = "Tested Read / Write successful.";

        private readonly Button testButton;
        private readonly Font font;
        private bool isRunning;
        private TextFlow textFlow;

        public SdWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.testButton = new Button() {
                Child = new Text(this.font, "Test Sd") {
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
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {
            this.textFlow.TextRuns.Clear();
            this.textFlow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0 && !this.isRunning) {
                this.ClearScreen();
                this.CreateWindow(false);
                this.textFlow.TextRuns.Clear();
                new Thread(this.ThreadTest).Start();
            }
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.isRunning = false;
            this.ClearScreen();
            this.CreateWindow(true);
        }

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
            this.Deinitialize();
        }

        private void ClearScreen() {
            this.canvas.Children.Clear();

            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0);
                Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0);
                Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                // Register touch event for Back / Next.
                this.OnBottomBarButtonBackTouchUpEvent += this.OnButtonBack;
                this.OnBottomBarButtonNextTouchUpEvent += this.OnButtonNext;
            }
        }

        private void CreateWindow(bool enableButton) {
            const int startX = 5;
            const int startY = 40;

            Canvas.SetLeft(this.textFlow, startX);
            Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);

            if (enableButton) {
                var buttonY = this.Height - ((this.testButton.Height * 3) / 2);
                Canvas.SetLeft(this.testButton, startX);
                Canvas.SetTop(this.testButton, buttonY);
                this.canvas.Children.Add(this.testButton);
            }
        }

        private void ThreadTest() {
            const int BlockSize = 1024;

            this.isRunning = true;

            var data = Encoding.UTF8.GetBytes("This is for sd  \n");
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
                this.UpdateStatusText(BadConnect, true);
                this.isRunning = false;
                return;
            }

            Thread.Sleep(1000);

            var filename = drive.Name + "\\TEST_SD.TXT";

            try {
                try {
                    using (var fsWrite = new FileStream(filename, FileMode.Create)) {
                        fsWrite.Write(dataWrite, 0, dataWrite.Length);
                        fsWrite.Flush();
                    }
                }
                catch {
                    this.UpdateStatusText(BadWrite, false);
                    return;
                }

                try {
                    using (var fsRead = new FileStream(filename, FileMode.Open)) {
                        fsRead.Read(dataRead, 0, dataRead.Length);

                        for (var i = 0; i < dataRead.Length; i++) {
                            if (dataRead[i] != dataWrite[i]) {
                                this.UpdateStatusText(DataCorrupted, false);
                                return;
                            }
                        }
                    }
                }
                catch {
                    this.UpdateStatusText(BadRead, false);
                    return;
                }

                this.UpdateStatusText(TestSuccess, false);
            }
            finally {
                try {
                    FileSystem.Flush(storageController.Hdc);
                    FileSystem.Unmount(storageController.Hdc);
                }
                catch {
                }

                this.isRunning = false;
            }
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, SystemDrawing.Color.White);
    }
}
