using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Cryptography;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class BasicTestWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This step will do simple test on:";
        private const string Instruction2 = " - User leds";
        private const string Instruction3 = " - User buttons";
        private const string Instruction4 = " - External RAM / Flash";
        private const string Instruction5 = " - Wifi";
        private const string Instruction6 = " - Buzzer";
        private const string Instruction7 = " - Usb Host/ Micro Sd";
        private const string Instruction8 = " - Real time clock crystal";

        private const string MountSuccess = "Mounted successful.";
        private const string BadConnect1 = "Bad device or no connect.";


        private Font font;

        private bool isRunning;

        private TextFlow textFlow;

        private Button testButton;

        private Button nextButton;

        private bool ethernetConnect = false;

        private bool doNext = false;

        private bool doTestWifiPassed = false;

        public BasicTestWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.testButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Test") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.nextButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Next") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;
            this.nextButton.Click += this.NextButton_Click;
        }

        private void Initialize() {

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

            this.textFlow.TextRuns.Add(Instruction6, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction7, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction8, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
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

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                this.doNext = true;
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

            this.ethernetConnect = false;
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

        private void AddNextButton() {
            var startX = 5;
            var buttonY = this.Height - ((this.testButton.Height * 3) / 2);

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {

                Canvas.SetLeft(this.nextButton, startX); Canvas.SetTop(this.nextButton, buttonY);
                this.canvas.Children.Add(this.nextButton);

                return null;

            }, null);

            Thread.Sleep(100);

            this.doNext = false;
        }

        private void RemoveNextButton() {
            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {

                this.canvas.Children.Remove(this.nextButton);

                return null;

            }, null);

            Thread.Sleep(100);

            this.doNext = false;
        }

        private void ThreadTest() {
            this.isRunning = true;

            if (this.isRunning == true && this.DoTestExternalRam() == true) {
                this.doNext = false;
                if (this.isRunning == true && this.DoTestExternalFlash() == true) {
                    this.doNext = false;
                    if (this.isRunning == true && this.DoTestLeds() == true) {
                        this.doNext = false;
                        if (this.isRunning == true && this.DoTestButtons() == true) {
                            this.doNext = false;
                            if (this.isRunning == true && this.DoTestWifi() == true) {
                                this.doNext = false;
                                if (this.isRunning == true && this.DoTestBuzzer() == true) {
                                    this.doNext = false;
                                    if (this.isRunning == true && this.DoTestUsbHost()) {
                                        this.doNext = false;
                                        if (this.isRunning == true && this.DoTestSdcard() == true) {
                                            this.doNext = false;
                                            if (this.isRunning == true && this.DoTestRtc() == true) {
                                                this.doNext = false;

                                                this.UpdateStatusText("Last step is Gpio testing.", true);
                                                this.UpdateStatusText("Warning: This step will toggle all exposed gpio.", false, System.Drawing.Color.Yellow);
                                                this.UpdateStatusText("Only needed for production test.", false, System.Drawing.Color.Yellow);
                                                this.UpdateStatusText("Next to do gpio test, or Back to return main menu", false);

                                                this.AddNextButton();

                                                while (this.doNext == false && this.isRunning == true) {
                                                    Thread.Sleep(10);
                                                }

                                                var doGpioTest = this.doNext;

                                                this.RemoveNextButton();

                                                if (doGpioTest && this.isRunning == true) {
                                                    this.doNext = false;

                                                    this.UpdateStatusText("Testing gpio....", true);
                                                    this.UpdateStatusText("* Ignored: PB8, PB9 (I2C1 - Touch)", false, System.Drawing.Color.Yellow);
                                                    this.UpdateStatusText("* Ignored: PB3, PB4, PB5 (SPI3 - wifi) ", false, System.Drawing.Color.Yellow);

                                                    this.DoTestGpio();
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.isRunning = false;
        }

        private bool startSdRamTest = false;

        private void ThreadBlinkLed() {
            var gpioController = GpioController.GetDefault();

            var redLed = gpioController.OpenPin(SC20260.GpioPin.PB0);

            redLed.SetDriveMode(GpioPinDriveMode.Output);


            while (this.startSdRamTest && this.doNext == false && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

                Thread.Sleep(100);
            }

            redLed.Dispose();
        }

        private bool DoTestExternalRam() {
            var result = true;

            if (this.startSdRamTest)
                return false;

            this.startSdRamTest = true;
            new Thread(this.ThreadBlinkLed).Start();

            var externalRam1 = new UnmanagedBuffer(16 * 1024 * 1024);
            var externalRam2 = new UnmanagedBuffer(14 * 1024 * 1024);

            byte[] buf1 = null;
            byte[] buf2 = null;

            var useUnmanagedHeap = false;

            if (GHIElectronics.TinyCLR.Native.Memory.UnmanagedMemory.FreeBytes == 0 &&
                GHIElectronics.TinyCLR.Native.Memory.ManagedMemory.FreeBytes > 512 * 1024) {
                buf1 = new byte[16 * 1024 * 1024];
                buf2 = new byte[14 * 1024 * 1024];
            }
            else {
                buf1 = externalRam1.Bytes;
                buf2 = externalRam2.Bytes;

                useUnmanagedHeap = true;
            }


            var md5 = GHIElectronics.TinyCLR.Cryptography.MD5.Create();

            var hashValue = md5.ComputeHash(buf1); //data is a byte array.

            var rd = new Random(3);

            this.UpdateStatusText("Testing external ram. It will take ~ 2 seconds to get test result.", true, System.Drawing.Color.White);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("External ram test is starting...", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("Ram test is failed if the led stops blinking ", false);
            this.UpdateStatusText("(or the test stop at this step more than 2 seconds) ", false);

            Thread.Sleep(100);

            rd.NextBytes(buf2);

            try {

                var hashValue2 = md5.ComputeHash(buf1); //data is a byte array.

                for (var i = 0; i < hashValue.Length; i++) {
                    if (hashValue[i] != hashValue2[i]) {
                        result = false;
                    }
                }
            }
            catch {

                result = false;
            }

            if (useUnmanagedHeap) {
                externalRam1.Dispose();
                externalRam2.Dispose();
            }

            if (result)
                this.UpdateStatusText("Test external ram passed.", false);
            else
                this.UpdateStatusText("Test external ram failed.", false, System.Drawing.Color.Red);

            this.startSdRamTest = false;

            return result;

        }

        private bool DoTestExternalFlash() {
            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);
            var drive = storeController.Provider;
            var result = true;

            drive.Open();

            var sectorSize = drive.Descriptor.RegionSizes[0];

            var dataRead = new byte[sectorSize];
            var dataWrite = new byte[sectorSize];

            var rd = new Random();

            var roundTest = 0;
            var startSector = 0;
            var endSector = 8;

            var md5 = MD5.Create();

_again:
            if (roundTest == 1) {
                startSector = 4088;
                endSector = startSector + 8;
            }

            for (var s = startSector; s < endSector; s++) {
                md5.Clear();

                rd.NextBytes(dataWrite);

                var md5dataWrite = md5.ComputeHash(dataWrite);

                this.UpdateStatusText("Testing external flash.", true);

                var address = s * sectorSize;
                this.UpdateStatusText("External flash - Erasing sector " + s, false);
                // Erase
                drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                // Write
                this.UpdateStatusText("External flash - Writing sector " + s, false);
                drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                this.UpdateStatusText("External flash - Reading sector " + s, false);

                //Read to compare
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                md5.Clear();

                var md5Read = md5.ComputeHash(dataRead);

                for (var i = 0; i < md5Read.Length; i++) {
                    if (md5Read[i] != md5dataWrite[i]) {
                        this.UpdateStatusText("External flash - Compare failed at: " + s, false);
                        result = false;
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

            return result;
        }

        private bool DoTestWifi() {
            this.UpdateStatusText("Checking wifi firmware...", true);

            if (this.doTestWifiPassed)
                return true;

            var gpioController = GpioController.GetDefault();

            var resetPin = gpioController.OpenPin(SC20260.GpioPin.PC3);
            var csPin = gpioController.OpenPin(SC20260.GpioPin.PA6);
            var intPin = gpioController.OpenPin(SC20260.GpioPin.PF10);
            var enPin = gpioController.OpenPin(SC20260.GpioPin.PA8);

            enPin.SetDriveMode(GpioPinDriveMode.Output);
            resetPin.SetDriveMode(GpioPinDriveMode.Output);

            enPin.Write(GpioPinValue.Low);
            resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);

            enPin.Write(GpioPinValue.High);
            resetPin.Write(GpioPinValue.High);

            var result = false;

            var settings = new GHIElectronics.TinyCLR.Devices.Spi.SpiConnectionSettings() {
                ChipSelectLine = csPin,
                ClockFrequency = 4000000,
                Mode = GHIElectronics.TinyCLR.Devices.Spi.SpiMode.Mode0,
                ChipSelectType = GHIElectronics.TinyCLR.Devices.Spi.SpiChipSelectType.Gpio,
                ChipSelectHoldTime = TimeSpan.FromTicks(10),
                ChipSelectSetupTime = TimeSpan.FromTicks(10)
            };

            var networkCommunicationInterfaceSettings = new SpiNetworkCommunicationInterfaceSettings {
                SpiApiName = SC20260.SpiBus.Spi3,
                GpioApiName = "GHIElectronics.TinyCLR.NativeApis.STM32H7.GpioController\\0",
                SpiSettings = settings,
                InterruptPin = intPin,
                InterruptEdge = GpioPinEdge.FallingEdge,
                InterruptDriveMode = GpioPinDriveMode.InputPullUp,
                ResetPin = resetPin,
                ResetActiveState = GpioPinValue.Low
            };

            var networkInterfaceSetting = new WiFiNetworkInterfaceSettings() {
                Ssid = " ",
                Password = " ",
            };

            networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 122 });
            networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
            networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
            networkInterfaceSetting.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 75, 75, 75, 75 }), new IPAddress(new byte[] { 75, 75, 75, 76 }) };

            //networkInterfaceSetting.MacAddress = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
            networkInterfaceSetting.DhcpEnable = true;
            networkInterfaceSetting.DynamicDnsEnable = true;

            var networkController = NetworkController.FromName("GHIElectronics.TinyCLR.NativeApis.ATWINC15xx.NetworkController");

            networkController.SetInterfaceSettings(networkInterfaceSetting);
            networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);
            networkController.SetAsDefaultController();
            var firmware = Winc15x0Interface.GetFirmwareVersion();

            if (firmware.IndexOf("19.5.") == 0 || (firmware.IndexOf("19.6.") == 0)) {
                result = true;
            }

            resetPin.Dispose();
            csPin.Dispose();
            intPin.Dispose();
            enPin.Dispose();

            this.doTestWifiPassed = result;

            return result;
        }
        private bool DoTestButtons() {
            var gpioController = GpioController.GetDefault();

            var ldrButton = gpioController.OpenPin(SC20260.GpioPin.PE3);
            var appButton = gpioController.OpenPin(SC20260.GpioPin.PB7);

            ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            appButton.SetDriveMode(GpioPinDriveMode.InputPullUp);

            this.UpdateStatusText("Testing buttons.", true);


            this.UpdateStatusText("Wait for press LDR button ", false);
            while (ldrButton.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (ldrButton.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            this.UpdateStatusText("Wait for press APP button ", false);
            while (appButton.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (appButton.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            ldrButton.Dispose();
            appButton.Dispose();

            return true;
        }

        private bool DoTestLeds() {
            var gpioController = GpioController.GetDefault();

            var redLed = gpioController.OpenPin(SC20260.GpioPin.PB0);

            redLed.SetDriveMode(GpioPinDriveMode.Output);

            this.UpdateStatusText("Testing the red led.", true);
            this.UpdateStatusText("- The test is passed if red is blinking.", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("- Only press Next button if the led are blinking.", false, System.Drawing.Color.Yellow);

            this.AddNextButton();

            while (this.doNext == false && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

                Thread.Sleep(100);
            }

            redLed.Dispose();

            this.RemoveNextButton();

            return true;
        }

        private bool DoTestUsbHost() {

            var result = true;
            this.UpdateStatusText("Waiting for usb host initialize...", true);

            UsbWindow.InitializeUsbHostController();

            while (!UsbWindow.IsUsbHostConnected && this.isRunning) Thread.Sleep(100);

            if (this.isRunning == false)
                return false;

            var storageController = StorageController.FromName(SC20260.StorageController.UsbHostMassStorage);

            IDriveProvider drive;

            try {
                drive = FileSystem.Mount(storageController.Hdc);

                var driveInfo = new DriveInfo(drive.Name);


                this.UpdateStatusText(MountSuccess, false);

            }
            catch {

                this.UpdateStatusText("Usb Host: " + BadConnect1, true);

                result = false;

                goto _return;
            }

_return:

            try {

                GHIElectronics.TinyCLR.IO.FileSystem.Flush(storageController.Hdc);
                GHIElectronics.TinyCLR.IO.FileSystem.Unmount(storageController.Hdc);
            }
            catch {

            }

            return result;
        }

        private bool DoTestSdcard() {

            var result = true;

            this.UpdateStatusText("Waiting for Sd initialize...", true);

            var storageController = StorageController.FromName(SC20260.StorageController.SdCard);

            IDriveProvider drive;
try_again:

            if (this.isRunning == false) {
                result = false;

                goto _return;
            }

            try {
                drive = FileSystem.Mount(storageController.Hdc);

                var driveInfo = new DriveInfo(drive.Name);


                this.UpdateStatusText(MountSuccess, false);

            }
            catch {

                this.UpdateStatusText("Sd: " + BadConnect1, true);

                while (this.doNext == false) {

                    Thread.Sleep(1000);

                    goto try_again;
                }

                result = false;

                goto _return;
            }

_return:

            try {

                GHIElectronics.TinyCLR.IO.FileSystem.Flush(storageController.Hdc);
                GHIElectronics.TinyCLR.IO.FileSystem.Unmount(storageController.Hdc);
            }
            catch {

            }

            return result;
        }

        private bool DoTestBuzzer() {

            this.UpdateStatusText("Testing buzzer...", true);

            using (var pwmController3 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id)) {

                var pwmPinPB1 = pwmController3.OpenChannel(SC20260.Timer.Pwm.Controller3.PB1);

                pwmController3.SetDesiredFrequency(500);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.UpdateStatusText("Generate Pwm 500Hz...", false);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                this.UpdateStatusText("Generate Pwm 1000Hz...", false);

                pwmController3.SetDesiredFrequency(1000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                this.UpdateStatusText("Generate Pwm 2000Hz...", false);

                pwmController3.SetDesiredFrequency(2000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Testing is success if you heard three different sounds!", false, System.Drawing.Color.Yellow);
            }

            this.AddNextButton();

            while (this.doNext == false && this.isRunning) {
                Thread.Sleep(100);
            }

            this.RemoveNextButton();

            return true;
        }

        private bool DoTestRtc() {
            this.UpdateStatusText("Testing real time clock... ", true);
try_again:
            try {
                var rtc = RtcController.GetDefault();

                var m = new DateTime(2020, 7, 7, 00, 00, 00);

                if (rtc.IsValid && rtc.Now >= m) {

                    return true;
                }

                else {
                    var newDt = RtcDateTime.FromDateTime(m);

                    rtc.SetTime(newDt);

                    if (rtc.IsValid && rtc.Now >= m) {

                        return true;
                    }
                }
            }
            catch {
                this.UpdateStatusText("Try Rtc again... ", true);
            }

            if (this.isRunning)
                goto try_again;

            return false;
        }


        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);

        private void DoTestGpio() {
            var pinsDefs = new int[] {SC20260.GpioPin.PD7, SC20260.GpioPin.PB13, SC20260.GpioPin.PB12, SC20260.GpioPin.PF7, SC20260.GpioPin.PF6, SC20260.GpioPin.PH12,SC20260.GpioPin.PE4,SC20260.GpioPin.PI4, SC20260.GpioPin.PA0, SC20260.GpioPin.PA4, SC20260.GpioPin.PJ8, SC20260.GpioPin.PJ9, SC20260.GpioPin.PA5, SC20260.GpioPin.PE6, SC20260.GpioPin.PH13, SC20260.GpioPin.PH14, SC20260.GpioPin.PC6, SC20260.GpioPin.PC7,
                SC20260.GpioPin.PH9, SC20260.GpioPin.PG10, SC20260.GpioPin.PA3, SC20260.GpioPin.PH10, SC20260.GpioPin.PF9, SC20260.GpioPin.PF8, SC20260.GpioPin.PJ10, SC20260.GpioPin.PJ11, SC20260.GpioPin.PK0,
                SC20260.GpioPin.PC0, SC20260.GpioPin.PD4, SC20260.GpioPin.PI1, SC20260.GpioPin.PI2, SC20260.GpioPin.PI3, SC20260.GpioPin.PC13, SC20260.GpioPin.PE5, SC20260.GpioPin.PC2, SC20260.GpioPin.PD6, SC20260.GpioPin.PD5
                               };

            var gpioController = GpioController.GetDefault();

            var gpios = new GpioPin[pinsDefs.Length];

            for (var i = 0; i < pinsDefs.Length; i++) {
                try {
                    gpios[i] = gpioController.OpenPin(pinsDefs[i]);
                    gpios[i].SetDriveMode(GpioPinDriveMode.Output);
                }
                catch {
                    this.UpdateStatusText(" Gpio test failed at: " + GetGpioPinName(pinsDefs[i]), true);
                    goto _return;
                }
            }

            var idx = 0;

            while (this.doNext == false && this.isRunning) {
                for (var i = 0; i < pinsDefs.Length; i++) {
                    if (i == idx) {
                        gpios[i].Write(gpios[i].Read() == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);
                    }
                    else {
                        gpios[i].Write(GpioPinValue.Low);
                    }
                }

                idx++;

                if (idx == pinsDefs.Length)
                    idx = 0;

                Thread.Sleep(300 / pinsDefs.Length);
            }
_return:
            for (var i = 0; i < pinsDefs.Length; i++) {
                if (gpios[i] != null)
                    gpios[i].Dispose();
            }
        }

        static string GetGpioPinName(int pinId) {
            var port = (char)((pinId / 16) + 'A');
            var pin = pinId % 16;
            return "P" + port + "" + pin;
        }
    }
}
