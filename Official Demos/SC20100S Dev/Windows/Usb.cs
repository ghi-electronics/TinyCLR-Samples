using System;
using System.IO;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.UsbHost;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class UsbWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;
        private bool isRunning;

        private static bool enabledUsbHost;
        private static bool usbConnected;

        private const string FreeSize      = "Total Free: ";
        private const string TotalSize     = "Total Size: ";
        private const string VolumeLabel   = "VolumeLabel: ";
        private const string RootDirectory = "RootDirectory: ";
        private const string DriveFormat   = "DriveFormat: ";

        private const string Instruction1 = "This test will write 1K of data ";
        private const string Instruction2 = "to the file TEST_USB.TXT, then ";
        private const string Instruction3 = "read back to compare data.";
        private const string Instruction4 = " ";
        private const string Instruction5 = "Insert USB drive and press Next";
        private const string Instruction6 = "button when you are ready.";

        private const string BadConnect    = "Bad device or no connect.";
        private const string DataCorrupted = "Data corrupted.";
        private const string BadWrite      = "Write failed.";
        private const string BadRead       = "Read failed.";
        private const string MountSuccess  = "Mounted successful.";
        private const string TestSuccess   = "Tested Read / Write successful.";

        public UsbWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
        }

        private void Initialize() {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg08);
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
            this.AppendInstruction(Instruction6);
        }

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void Deinitialize() {
            if (this.BottomBar != null)
                this.OnBottomBarButtonUpEvent -= this.OnHardwareButtonUp;

            this.textFlow.TextRuns.Clear();
            this.canvas.Children.Clear();
            this.font.Dispose();
            this.textFlow = null;
            this.canvas = null;
        }

        protected override void Active() {
            this.Initialize();
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.ClearScreen();
            this.CreateWindow();
            InitializeUsbHostController();
        }

        public static void InitializeUsbHostController() {
            if (enabledUsbHost) return;
            enabledUsbHost = true;

            var host = UsbHostController.GetDefault();
            host.OnConnectionChangedEvent += (sender, e) =>
                usbConnected = e.DeviceStatus == DeviceConnectionStatus.Connected;
            host.Enable();
        }

        public static bool IsUsbHostConnected => usbConnected;

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
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
                this.OnBottomBarButtonUpEvent += this.OnHardwareButtonUp;
            }
        }

        private void OnHardwareButtonUp(object sender, RoutedEventArgs e) {
            var buttonSource = (ButtonEventArgs)e;
            switch (buttonSource.Button) {
                case HardwareButton.Left:
                    this.Close();
                    break;
                case HardwareButton.Right:
                case HardwareButton.Select:
                    if (!this.isRunning)
                        new Thread(this.ThreadTest).Start();
                    break;
            }
        }

        private void CreateWindow() {
            Canvas.SetLeft(this.textFlow, 5);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }

        private void ThreadTest() {
            const int BlockSize = 1024;
            this.isRunning = true;

            var data = Encoding.UTF8.GetBytes("This is for usb\n");
            var dataWrite = new byte[BlockSize];
            var dataRead = new byte[BlockSize];

            for (var i = 0; i < BlockSize; i += data.Length)
                Array.Copy(data, 0, dataWrite, i, data.Length);

            var storageController = StorageController.FromName(SC20100.StorageController.UsbHostMassStorage);

            // Wait up to 5s for a stick to be plugged in.
            for (var t = 0; t < 5 && !usbConnected; t++)
                Thread.Sleep(1000);

            if (!usbConnected) {
                this.UpdateStatusText(BadConnect, true);
                this.isRunning = false;
                return;
            }

            try {
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
                    return;
                }

                var filename = drive.Name + "\\TEST_USB.TXT";

                try {
                    using (var fs = new FileStream(filename, FileMode.Create)) {
                        fs.Write(dataWrite, 0, dataWrite.Length);
                        fs.Flush();
                    }
                }
                catch {
                    this.UpdateStatusText(BadWrite, false);
                    return;
                }

                try {
                    using (var fs = new FileStream(filename, FileMode.Open)) {
                        fs.Read(dataRead, 0, dataRead.Length);
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
