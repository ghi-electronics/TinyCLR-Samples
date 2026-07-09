using System;
using System.Drawing;
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
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    // PORTED TO THE UI DESIGNER. The layout (status TextFlow + Test button) is designed in
    // UsbContent.tcui; this window stays an ApplicationWindow (so it keeps its place in the menu
    // navigation), hosts that fragment, and drives its elements with the same USB read/write test logic
    // as before. The static USB-host members (InitializeUsbHostController / IsUsbHostConnected) used by
    // other windows are kept exactly.
    public class UsbWindow : ApplicationWindow {
        private Canvas canvas;

        private const string FreeSize = "Total Free: ";
        private const string TotalSize = "Total Size: ";
        private const string VolumeLabel = "VolumeLabel: ";
        private const string RootDirectory = "RootDirectory: ";
        private const string DriveFormat = "DriveFormat: ";

        private const string Instruction1 = "This test will write 1K of data to the file TEST_USB.TXT,";
        private const string Instruction2 = "then read back to compare data.";
        private const string Instruction3 = "Insert usb stick and press Test Button when you are ready.";

        private const string BadConnect = "Bad device or no connect.";
        private const string DataCorrupted = "Data corrupted.";
        private const string BadWrite = "Write failed.";
        private const string BadRead = "Read failed.";

        private const string MountSuccess = "Mounted successful.";
        private const string TestSuccess = "Tested Read / Write successful.";

        private readonly Font font;

        private static bool enabledUsbHost;
        private static bool usbConnected;

        private bool isRunning;
        private TextFlow textFlow;   // the designed StatusText, grabbed from the fragment
        private Button testButton;   // the designed TestButton, grabbed from the fragment

        public UsbWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);
        }

        public static bool IsUsbHostConnected => usbConnected;

        protected override void Active() {
            var content = new UsbContent();
            this.canvas = content;
            this.textFlow = content.StatusText;
            this.testButton = content.TestButton;
            this.testButton.Click += this.TestButton_Click;
            this.isRunning = false;

            this.AddChrome();
            this.Child = this.canvas;

            this.textFlow.TextRuns.Clear();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);

            InitializeUsbHostController();
        }

        // Adds the shared TopBar/BottomBar chrome (unchanged from the original) to the fragment's canvas.
        private void AddChrome() {
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

        private void AppendInstruction(string text) {
            this.textFlow.TextRuns.Add(text, this.font, Colors.White);
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (!this.isRunning) {
                this.testButton.Visibility = Visibility.Collapsed; // hide while running (original removed it)
                this.textFlow.TextRuns.Clear();
                new Thread(this.ThreadTest).Start();
            }
        }

        public static void InitializeUsbHostController() {
            if (!enabledUsbHost) {
                enabledUsbHost = true;

                var usbHostController = UsbHostController.GetDefault();
                usbHostController.OnConnectionChangedEvent += UsbHostController_OnConnectionChangedEvent;
                usbHostController.Enable();
            }
        }

        private static void UsbHostController_OnConnectionChangedEvent(UsbHostController sender, DeviceConnectionEventArgs e) =>
            usbConnected = e.DeviceStatus == DeviceConnectionStatus.Connected;

        private void OnButtonBack(object sender, RoutedEventArgs e) => this.Close();
        private void OnButtonNext(object sender, RoutedEventArgs e) => this.Close();

        protected override void Deactive() {
            this.isRunning = false;
            Thread.Sleep(10);
            this.canvas.Children.Clear();
        }

        private void ThreadTest() {
            const int BlockSize = 1024;

            this.isRunning = true;

            var data = Encoding.UTF8.GetBytes("This is for usb\n");
            var dataWrite = new byte[BlockSize];
            var dataRead = new byte[BlockSize];

            for (var i = 0; i < BlockSize; i += data.Length) {
                Array.Copy(data, 0, dataWrite, i, data.Length);
            }

            var storageController = StorageController.FromName(SC20260.StorageController.UsbHostMassStorage);
            IDriveProvider drive;

            // Wait up to 5 seconds for a USB device to be plugged in.
            var timeout = 0;
            while (!usbConnected) {
                Thread.Sleep(1000);
                if (++timeout > 5)
                    break;
            }

            if (!usbConnected) {
                this.UpdateStatusText(BadConnect, true);
                this.isRunning = false;
                return;
            }

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

            var filename = drive.Name + "\\TEST_USB.TXT";

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
