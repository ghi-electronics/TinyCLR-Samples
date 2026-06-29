using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Drivers.Omnivision.Ov9655;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class BasicTestWindow : ApplicationWindow {
        private Canvas canvas;

        private const string Instruction1 = "This step will do simple test on:";
        private const string Instruction2 = " - User leds";
        private const string Instruction3 = " - User buttons";
        private const string Instruction4 = " - External RAM / Flash";
        private const string Instruction5 = " - Ethernet";
        private const string Instruction6 = " - Buzzer";
        private const string Instruction7 = " - Usb Host / Micro SD";
        private const string Instruction8 = " - Real time clock crystal";
        private const string Instruction9 = " - Camera Interface";
        private const string Instruction10 = " - VCOM Uart5";
        private const string Instruction11 = " - CAN1 FD";

        private const string MountSuccess = "Mounted successful.";
        private const string BadConnect = "Bad device or no connect.";

        private readonly Font font;
        private readonly Button testButton;
        private readonly Button nextButton;

        private bool isRunning;
        private bool ethernetConnect;
        private bool doNext;
        private bool startSdRamTest;

        private TextFlow textFlow;

        public BasicTestWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.testButton = MakeButton("Test");
            this.nextButton = MakeButton("Next");

            this.testButton.Click += this.TestButton_Click;
            this.nextButton.Click += this.NextButton_Click;
        }

        private Button MakeButton(string text) => new Button() {
            Child = new Text(this.font, text) {
                ForeColor = Colors.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            Width = 100,
            Height = 30,
        };

        private void NextButton_Click(object sender, RoutedEventArgs e) => this.doNext = true;

        private void Initialize() {
            this.textFlow = new TextFlow();
            this.AppendInstruction(Instruction1);
            this.AppendInstruction(Instruction2);
            this.AppendInstruction(Instruction3);
            this.AppendInstruction(Instruction4);
            this.AppendInstruction(Instruction5);
            this.AppendInstruction(Instruction6);
            this.AppendInstruction(Instruction7);
            this.AppendInstruction(Instruction8);
            this.AppendInstruction(Instruction9);
            this.AppendInstruction(Instruction10);
            this.AppendInstruction(Instruction11);
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
            if (!this.isRunning) {
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
            this.ethernetConnect = false;
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

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(text, clearScreen, SystemDrawing.Color.White);

        private void UpdateStatusText(string text, bool clearScreen, SystemDrawing.Color color) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, color);

        private void ThreadTest() {
            this.isRunning = true;

            try {
                if (!this.RunStep(this.DoTestExternalRam)) return;
                if (!this.RunStep(this.DoTestExternalFlash)) return;
                if (!this.RunStep(this.DoTestEthernet)) return;
                if (!this.RunStep(this.DoTestLeds)) return;
                if (!this.RunStep(this.DoTestButtons)) return;
                if (!this.RunStep(this.DoTestBuzzer)) return;
                if (!this.RunStep(this.DoTestUsbHost)) return;
                if (!this.RunStep(this.DoTestSdcard)) return;
                if (!this.RunStep(this.DoTestRtc)) return;

                var testUart = this.DoTestUart();
                this.doNext = false;
                var testCan = this.DoTestCan();
                this.doNext = false;

                this.UpdateStatusText(Instruction10 + (testUart ? ": Passed" : ": Failed"), true,  testUart ? SystemDrawing.Color.White : SystemDrawing.Color.Red);
                this.UpdateStatusText(Instruction11 + (testCan  ? ": Passed" : ": Failed"), false, testCan  ? SystemDrawing.Color.White : SystemDrawing.Color.Red);

                this.UpdateStatusText("Last step is GPIO testing.", false);
                this.UpdateStatusText("Warning: this step will toggle all exposed GPIO.", false, SystemDrawing.Color.Yellow);
                this.UpdateStatusText("Only needed for production test.", false, SystemDrawing.Color.Yellow);
                this.UpdateStatusText("Next to do GPIO test, or Back to return to main menu.", false);

                this.AddNextButton();
                while (!this.doNext && this.isRunning) Thread.Sleep(10);
                var doGpioTest = this.doNext;
                this.RemoveNextButton();

                if (doGpioTest && this.isRunning) {
                    this.doNext = false;
                    this.UpdateStatusText("Testing GPIO....", true);
                    this.UpdateStatusText("* Ignored: PB8, PB9 (I2C1 - Touch)", false, SystemDrawing.Color.Yellow);
                    this.UpdateStatusText("* Ignored: X2 header (LCD)", false, SystemDrawing.Color.Yellow);
                    this.UpdateStatusText("* Do NOT forget to test external power *", false, SystemDrawing.Color.Yellow);
                    this.DoTestGpio();
                }
            }
            finally {
                this.isRunning = false;
            }
        }

        private bool RunStep(Func<bool> step) {
            if (!this.isRunning || !step()) return false;
            this.doNext = false;
            return true;
        }

        private void ThreadBlinkLed() {
            var gpioController = GpioController.GetDefault();
            var redLed = gpioController.OpenPin(SC20260.GpioPin.PB0);
            redLed.SetDriveMode(GpioPinDriveMode.Output);

            while (this.startSdRamTest && !this.doNext && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                Thread.Sleep(100);
            }

            redLed.Dispose();
        }

        private bool DoTestExternalRam() {
            if (this.startSdRamTest)
                return false;

            this.startSdRamTest = true;
            new Thread(this.ThreadBlinkLed).Start();

            UnmanagedBuffer externalRam1 = null;
            UnmanagedBuffer externalRam2 = null;
            byte[] buf1;
            byte[] buf2;
            var useUnmanagedHeap = false;

            if (Memory.UnmanagedMemory.FreeBytes == 0 && Memory.ManagedMemory.FreeBytes > 512 * 1024) {
                buf1 = new byte[16 * 1024 * 1024];
                buf2 = new byte[14 * 1024 * 1024];
            }
            else {
                externalRam1 = new UnmanagedBuffer(16 * 1024 * 1024);
                externalRam2 = new UnmanagedBuffer(14 * 1024 * 1024);
                buf1 = externalRam1.Bytes;
                buf2 = externalRam2.Bytes;
                useUnmanagedHeap = true;
            }

            var md5 = MD5.Create();
            var hashValue = md5.ComputeHash(buf1);

            this.UpdateStatusText("Testing external RAM. It will take ~ 2 seconds to get a test result.", true);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("External RAM test is starting...", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("RAM test failed if the led stops blinking", false);
            this.UpdateStatusText("(or the test stops at this step for more than 2 seconds).", false);

            Thread.Sleep(100);
            new Random(3).NextBytes(buf2);

            var result = true;
            try {
                var hashValue2 = md5.ComputeHash(buf1);
                for (var i = 0; i < hashValue.Length; i++) {
                    if (hashValue[i] != hashValue2[i]) {
                        result = false;
                        break;
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
                this.UpdateStatusText("Test external RAM passed.", false);
            else
                this.UpdateStatusText("Test external RAM failed.", false, SystemDrawing.Color.Red);

            this.startSdRamTest = false;
            return result;
        }

        private bool DoTestExternalFlash() {
            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);
            var drive = storeController.Provider;
            drive.Open();

            try {
                var sectorSize = drive.Descriptor.RegionSizes[0];
                var dataRead = new byte[sectorSize];
                var dataWrite = new byte[sectorSize];
                var rd = new Random();
                var md5 = MD5.Create();

                int[] sectorStarts = { 0, 4088 };
                foreach (var startSector in sectorStarts) {
                    var endSector = startSector + 8;
                    for (var s = startSector; s < endSector; s++) {
                        md5.Initialize();
                        rd.NextBytes(dataWrite);
                        var md5dataWrite = md5.ComputeHash(dataWrite);

                        this.UpdateStatusText("Testing external flash.", true);

                        var address = s * sectorSize;
                        this.UpdateStatusText("External flash - Erasing sector " + s, false);
                        drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                        this.UpdateStatusText("External flash - Writing sector " + s, false);
                        drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                        this.UpdateStatusText("External flash - Reading sector " + s, false);
                        drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                        md5.Initialize();
                        var md5Read = md5.ComputeHash(dataRead);
                        for (var i = 0; i < md5Read.Length; i++) {
                            if (md5Read[i] != md5dataWrite[i]) {
                                this.UpdateStatusText("External flash - Compare failed at: " + s, false);
                                return false;
                            }
                        }
                    }
                }

                this.UpdateStatusText("Tested Quad Spi successful!", false);
                return true;
            }
            finally {
                drive.Close();
            }
        }

        private bool DoTestEthernet() {
            var gpioController = GpioController.GetDefault();
            var resetPin = gpioController.OpenPin(SC20260.GpioPin.PG3);
            resetPin.SetDriveMode(GpioPinDriveMode.Output);

            resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            resetPin.Write(GpioPinValue.High);
            Thread.Sleep(100);

            var networkController = NetworkController.FromName("GHIElectronics.TinyCLR.NativeApis.STM32H7.EthernetEmacController\\0");

            var networkInterfaceSetting = new EthernetNetworkInterfaceSettings {
                Address = new IPAddress(new byte[] { 192, 168, 1, 122 }),
                SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 }),
                GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 }),
                DnsAddresses = new[] {
                    new IPAddress(new byte[] { 75, 75, 75, 75 }),
                    new IPAddress(new byte[] { 75, 75, 75, 76 }),
                },
                MacAddress = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 },
                DhcpEnable = true,
                DynamicDnsEnable = true,
            };

            networkController.SetInterfaceSettings(networkInterfaceSetting);
            networkController.SetCommunicationInterfaceSettings(new BuiltInNetworkCommunicationInterfaceSettings());
            networkController.SetAsDefaultController();
            networkController.NetworkAddressChanged += this.NetworkController_NetworkAddressChanged;
            networkController.NetworkLinkConnectedChanged += this.NetworkController_NetworkLinkConnectedChanged;

            var start = DateTime.Now;
            networkController.Enable();

            while (!this.ethernetConnect && this.isRunning) {
                this.UpdateStatusText("Testing ethernet. If connecting takes more than 10 seconds, the test failed.", true);
                this.UpdateStatusText(" ", false);
                this.UpdateStatusText(" - If connecting takes more than 10 seconds, check the cable.", false);
                this.UpdateStatusText(" ", false);
                this.UpdateStatusText("Please wait for connecting.... " + (int)(DateTime.Now - start).TotalSeconds, false);
                Thread.Sleep(1000);
            }

            networkController.Disable();
            networkController.Dispose();
            resetPin.Dispose();
            return this.ethernetConnect;
        }

        private void NetworkController_NetworkLinkConnectedChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e) {
            // Link state changes intentionally do not refresh the UI; the
            // address-changed event below is the trigger for that.
        }

        private void NetworkController_NetworkAddressChanged(NetworkController sender, NetworkAddressChangedEventArgs e) {
            var ipProperties = sender.GetIPProperties();
            var address = ipProperties.Address.GetAddressBytes();
            var subnet = ipProperties.SubnetMask.GetAddressBytes();
            var gw = ipProperties.GatewayAddress.GetAddressBytes();

            var ip = address[0] + "." + address[1] + "." + address[2] + "." + address[3];
            var gateway = gw[0] + "." + gw[1] + "." + gw[2] + "." + gw[3];
            var subnetmask = subnet[0] + "." + subnet[1] + "." + subnet[2] + "." + subnet[3];

            if (address[0] != 0) {
                this.UpdateStatusText("ip: " + ip, false);
                this.UpdateStatusText("gateway: " + gateway, false);
                this.UpdateStatusText("subnetmask: " + subnetmask, false);
            }

            this.ethernetConnect = address[0] != 0;
        }

        private bool DoTestButtons() {
            var gpioController = GpioController.GetDefault();
            var ldrButton = gpioController.OpenPin(SC20260.GpioPin.PE3);
            var appButton = gpioController.OpenPin(SC20260.GpioPin.PB7);
            var modeButton = gpioController.OpenPin(SC20260.GpioPin.PD7);

            ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            appButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            modeButton.SetDriveMode(GpioPinDriveMode.InputPullUp);

            this.UpdateStatusText("Testing buttons.", true);

            this.WaitForButtonPressAndRelease(ldrButton, "LDR");
            this.WaitForButtonPressAndRelease(appButton, "APP");
            this.WaitForButtonPressAndRelease(modeButton, "MOD");

            ldrButton.Dispose();
            appButton.Dispose();
            modeButton.Dispose();
            return true;
        }

        private void WaitForButtonPressAndRelease(GpioPin pin, string name) {
            this.UpdateStatusText("Wait for press " + name + " button", false);
            while (pin.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (pin.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);
        }

        private bool DoTestLeds() {
            var gpioController = GpioController.GetDefault();
            var redLed = gpioController.OpenPin(SC20260.GpioPin.PB0);
            var greenLed = gpioController.OpenPin(SC20260.GpioPin.PH11);

            redLed.SetDriveMode(GpioPinDriveMode.Output);
            greenLed.SetDriveMode(GpioPinDriveMode.Output);

            this.UpdateStatusText("Testing red and green leds.", true);
            this.UpdateStatusText("- The test passes if red and green led are blinking.", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("- Only press Next button if those leds are blinking.", false, SystemDrawing.Color.Yellow);

            this.AddNextButton();
            while (!this.doNext && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                greenLed.Write(greenLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                Thread.Sleep(100);
            }

            redLed.Dispose();
            greenLed.Dispose();
            this.RemoveNextButton();
            return true;
        }

        private bool DoTestUsbHost() {
            this.UpdateStatusText("Waiting for USB host initialize...", true);

            UsbWindow.InitializeUsbHostController();
            while (!UsbWindow.IsUsbHostConnected && this.isRunning) Thread.Sleep(100);

            if (!this.isRunning)
                return false;

            var storageController = StorageController.FromName(SC20260.StorageController.UsbHostMassStorage);
            var result = true;

            try {
                var drive = FileSystem.Mount(storageController.Hdc);
                _ = new DriveInfo(drive.Name);
                this.UpdateStatusText(MountSuccess, false);
            }
            catch {
                this.UpdateStatusText("Usb Host: " + BadConnect, true);
                result = false;
            }
            finally {
                try {
                    FileSystem.Flush(storageController.Hdc);
                    FileSystem.Unmount(storageController.Hdc);
                }
                catch {
                }
            }

            return result;
        }

        private bool DoTestSdcard() {
            this.UpdateStatusText("Waiting for SD initialize...", true);

            var storageController = StorageController.FromName(SC20260.StorageController.SdCard);
            var result = true;
            var mounted = false;

            try {
                while (!mounted && this.isRunning) {
                    try {
                        var drive = FileSystem.Mount(storageController.Hdc);
                        _ = new DriveInfo(drive.Name);
                        this.UpdateStatusText(MountSuccess, false);
                        mounted = true;
                    }
                    catch {
                        this.UpdateStatusText("SD: " + BadConnect, true);
                        if (this.doNext) {
                            result = false;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }

                if (!this.isRunning)
                    result = false;
            }
            finally {
                try {
                    FileSystem.Flush(storageController.Hdc);
                    FileSystem.Unmount(storageController.Hdc);
                }
                catch {
                }
            }

            return result;
        }

        private bool DoTestBuzzer() {
            this.UpdateStatusText("Testing buzzer...", true);

            using (var pwmController3 = PwmController.FromName(SC20260.Timer.Pwm.Controller3.Id)) {
                var pwmPinPB1 = pwmController3.OpenChannel(SC20260.Timer.Pwm.Controller3.PB1);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.PlayTone(pwmController3, pwmPinPB1, 500, "Generate Pwm 500Hz...");
                this.PlayTone(pwmController3, pwmPinPB1, 1000, "Generate Pwm 1000Hz...");
                this.PlayTone(pwmController3, pwmPinPB1, 2000, "Generate Pwm 2000Hz...");

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Test passes if you heard three different tones!", false, SystemDrawing.Color.Yellow);
            }

            this.AddNextButton();
            while (!this.doNext && this.isRunning) Thread.Sleep(100);
            this.RemoveNextButton();
            return true;
        }

        private void PlayTone(PwmController controller, PwmChannel channel, int frequency, string status) {
            this.UpdateStatusText(status, false);
            controller.SetDesiredFrequency(frequency);
            channel.Start();
            Thread.Sleep(1000);
            channel.Stop();
        }

        private bool DoTestRtc() {
            this.UpdateStatusText("Testing real time clock...", true);

            var reference = new DateTime(2020, 7, 7, 0, 0, 0);

            while (this.isRunning) {
                try {
                    var rtc = RtcController.GetDefault();
                    if (rtc.IsValid && rtc.Now >= reference)
                        return true;

                    rtc.SetTime(RtcDateTime.FromDateTime(reference));
                    if (rtc.IsValid && rtc.Now >= reference)
                        return true;
                }
                catch {
                    this.UpdateStatusText("Try RTC again...", true);
                }
            }

            return false;
        }

        private bool DoTestUart() {
            this.UpdateStatusText("Testing VCOM Uart5.", true);
            this.UpdateStatusText(" - Connect VCOM Uart5 to PC.", false);
            this.UpdateStatusText(" - Open Tera Term. Baudrate: 9600, Data: 8, Parity: None, StopBit: One.", false);
            this.UpdateStatusText(" - Type 'A' or 'a'.", false);
            this.UpdateStatusText(" - The test is waiting for any character on the Tera Term screen.", false);

            var result = false;

            using (var uart5 = UartController.FromName(SC20260.UartPort.Uart5)) {
                uart5.SetActiveSettings(new UartSetting { BaudRate = 9600 });
                uart5.Enable();

                this.AddNextButton();

                while (!this.doNext && this.isRunning) {
                    if (uart5.BytesToRead == 0) {
                        Thread.Sleep(100);
                        uart5.Write(new byte[] { (byte)'a' });
                        continue;
                    }

                    var byteToRead = uart5.BytesToRead > uart5.ReadBufferSize ? uart5.ReadBufferSize : uart5.BytesToRead;
                    var read = new byte[byteToRead];
                    uart5.Read(read);

                    for (var i = 0; i < read.Length; i++) {
                        if (read[i] == 'a') {
                            result = true;
                            break;
                        }
                    }

                    if (result) break;
                }
            }

            this.RemoveNextButton();
            return result;
        }

        private bool DoTestCan() {
            this.AddNextButton();

            this.UpdateStatusText("Testing CAN1.", true);
            this.UpdateStatusText("- Open PCAN-View application.", false);
            this.UpdateStatusText("- Nominal speed: 250Kbit/s.", false);
            this.UpdateStatusText("- Data speed: 500Kbit/s.", false);
            this.UpdateStatusText("- The test is waiting for any message with arbitrationId 0x1234.", false);

            var canController = CanController.FromName(SC20260.CanBus.Can1);
            canController.SetNominalBitTiming(new CanBitTiming(15 + 8, 8, 6, 8, false)); // 250Kbit/s
            canController.SetDataBitTiming(new CanBitTiming(15 + 8, 8, 3, 8, false));    // 500Kbit/s
            canController.Enable();

            var message = new CanMessage {
                ArbitrationId = 0x1234,
                ExtendedId = true,
                Data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                Length = 8,
            };

            var result = false;

            while (!this.doNext && this.isRunning) {
                if (canController.MessagesToRead == 0) {
                    try {
                        canController.WriteMessage(message);
                    }
                    catch {
                        canController.Disable();
                        Thread.Sleep(100);
                        canController.Enable();
                    }

                    Thread.Sleep(1000);
                    continue;
                }

                var msgs = new CanMessage[canController.MessagesToRead];
                for (var i = 0; i < msgs.Length; i++)
                    msgs[i] = new CanMessage();

                canController.ReadMessages(msgs, 0, msgs.Length);

                for (var i = 0; i < msgs.Length; i++) {
                    if (msgs[i].ArbitrationId == message.ArbitrationId) {
                        this.doNext = true;
                        result = true;
                        break;
                    }
                }

                if (result) break;
            }

            canController.Disable();
            this.RemoveNextButton();
            return result;
        }

        private void AddNextButton() {
            const int startX = 5;
            var buttonY = this.Height - ((this.testButton.Height * 3) / 2);

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {
                Canvas.SetLeft(this.nextButton, startX);
                Canvas.SetTop(this.nextButton, buttonY);
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

        private void DoTestGpio() {
            var pinsDefs = new[] {
                SC20260.GpioPin.PK0, SC20260.GpioPin.PJ11, SC20260.GpioPin.PJ10, SC20260.GpioPin.PI6,
                SC20260.GpioPin.PA10, SC20260.GpioPin.PA9, SC20260.GpioPin.PD6, SC20260.GpioPin.PD5,
                SC20260.GpioPin.PD4, SC20260.GpioPin.PD3, SC20260.GpioPin.PH6, SC20260.GpioPin.PI7,
                SC20260.GpioPin.PJ0, SC20260.GpioPin.PJ1, SC20260.GpioPin.PC0, SC20260.GpioPin.PA0,
                SC20260.GpioPin.PA3, SC20260.GpioPin.PA5, SC20260.GpioPin.PC3,
                SC20260.GpioPin.PH7, SC20260.GpioPin.PH8, SC20260.GpioPin.PF6, SC20260.GpioPin.PF7,
                SC20260.GpioPin.PF8, SC20260.GpioPin.PF9, SC20260.GpioPin.PB10, SC20260.GpioPin.PB11,
                SC20260.GpioPin.PG7, SC20260.GpioPin.PI1, SC20260.GpioPin.PI2, SC20260.GpioPin.PI3,
                SC20260.GpioPin.PA13, SC20260.GpioPin.PA14,
                SC20260.GpioPin.PF10, SC20260.GpioPin.PI8, SC20260.GpioPin.PG12, SC20260.GpioPin.PB3,
                SC20260.GpioPin.PB4, SC20260.GpioPin.PB5, SC20260.GpioPin.PI0, SC20260.GpioPin.PG6,
                SC20260.GpioPin.PJ9, SC20260.GpioPin.PJ8,
                SC20260.GpioPin.PC2, SC20260.GpioPin.PI11, SC20260.GpioPin.PC13, SC20260.GpioPin.PI5,
                SC20260.GpioPin.PJ13, SC20260.GpioPin.PC7, SC20260.GpioPin.PC6,
                SC20260.GpioPin.PJ7, SC20260.GpioPin.PH9, SC20260.GpioPin.PG10, SC20260.GpioPin.PE4,
                SC20260.GpioPin.PE5, SC20260.GpioPin.PA8, SC20260.GpioPin.PA4,
                SC20260.GpioPin.PI9, SC20260.GpioPin.PH10, SC20260.GpioPin.PH12, SC20260.GpioPin.PI4,
                SC20260.GpioPin.PE6, SC20260.GpioPin.PA6, SC20260.GpioPin.PG9,
            };

            var gpioController = GpioController.GetDefault();
            var gpios = new GpioPin[pinsDefs.Length];

            try {
                for (var i = 0; i < pinsDefs.Length; i++) {
                    try {
                        gpios[i] = gpioController.OpenPin(pinsDefs[i]);
                        gpios[i].SetDriveMode(GpioPinDriveMode.Output);
                    }
                    catch {
                        this.UpdateStatusText(" GPIO test failed at: " + GetGpioPinName(pinsDefs[i]), true);
                        return;
                    }
                }

                var idx = 0;
                while (!this.doNext && this.isRunning) {
                    for (var i = 0; i < pinsDefs.Length; i++) {
                        if (i == idx)
                            gpios[i].Write(gpios[i].Read() == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);
                        else
                            gpios[i].Write(GpioPinValue.Low);
                    }

                    idx = (idx + 1) % pinsDefs.Length;
                    Thread.Sleep(300 / pinsDefs.Length);
                }
            }
            finally {
                for (var i = 0; i < pinsDefs.Length; i++)
                    gpios[i]?.Dispose();
            }
        }

        private static string GetGpioPinName(int pinId) {
            var port = (char)((pinId / 16) + 'A');
            var pin = pinId % 16;
            return "P" + port + pin;
        }
    }
}
