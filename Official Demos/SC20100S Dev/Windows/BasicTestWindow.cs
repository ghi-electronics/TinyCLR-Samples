using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    internal class BasicTestWindow : ApplicationWindow {
        private Canvas canvas;
        private SystemDrawing.Font font;
        private TextFlow textFlow;

        private bool isRunning;
        private bool doNext;

        private const string Instruction1 = "This step will do simple test on:";
        private const string Instruction2 = " - User leds";
        private const string Instruction3 = " - External Flash";
        private const string Instruction4 = " - Buzzer";
        private const string Instruction5 = " - Usb Host / Micro SD";
        private const string Instruction6 = " - RTC crystal";
        private const string Instruction7 = " - VCOM Uart1";
        private const string Instruction8 = " - CAN1 FD";

        private const string MountSuccess = "Mounted successful.";
        private const string BadConnect   = "Bad device or no connect.";

        public BasicTestWindow(Resources.BitmapResources icon, string text, int width, int height) : base(icon, text, width, height) {
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
            this.AppendInstruction(Instruction7);
            this.AppendInstruction(Instruction8);
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
            this.isRunning = false;
            this.ClearScreen();
            this.CreateWindow();
        }

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
                    else
                        this.doNext = true;
                    break;
            }
        }

        private void CreateWindow() {
            Canvas.SetLeft(this.textFlow, 5);
            Canvas.SetTop(this.textFlow, 20);
            this.canvas.Children.Add(this.textFlow);
        }

        private void UpdateStatusText(string text, bool clearScreen) =>
            this.UpdateStatusText(text, clearScreen, SystemDrawing.Color.White);

        private void UpdateStatusText(string text, bool clearScreen, SystemDrawing.Color color) =>
            this.UpdateStatusText(this.textFlow, text, this.font, clearScreen, color);

        private void ThreadTest() {
            this.isRunning = true;
            this.doNext = false;

            try {
                if (!this.RunStep(this.DoTestLeds)) return;
                if (!this.RunStep(this.DoTestExternalFlash)) return;
                if (!this.RunStep(this.DoTestBuzzer)) return;
                if (!this.RunStep(this.DoTestUsbHost)) return;
                if (!this.RunStep(this.DoTestSdcard)) return;
                if (!this.RunStep(this.DoTestRtc)) return;

                var testUart = this.DoTestUart();
                this.doNext = false;
                var testCan = this.DoTestCan();
                this.doNext = false;

                this.UpdateStatusText(Instruction7 + (testUart ? ": Passed" : ": Failed"), true,  testUart ? SystemDrawing.Color.White : SystemDrawing.Color.Red);
                this.UpdateStatusText(Instruction8 + (testCan  ? ": Passed" : ": Failed"), false, testCan  ? SystemDrawing.Color.White : SystemDrawing.Color.Red);

                this.UpdateStatusText("Last step is GPIO testing.", false);
                this.UpdateStatusText("Warning: this step will toggle all", false, SystemDrawing.Color.Yellow);
                this.UpdateStatusText("exposed gpio.", false, SystemDrawing.Color.Yellow);
                this.UpdateStatusText("Only needed for production test.", false, SystemDrawing.Color.Yellow);
                this.UpdateStatusText("Next to do gpio test, or", false);
                this.UpdateStatusText("Back to return to main menu.", false);

                while (!this.doNext && this.isRunning)
                    Thread.Sleep(10);

                if (this.doNext && this.isRunning) {
                    this.doNext = false;
                    this.UpdateStatusText("Testing GPIO....", true);
                    this.UpdateStatusText("* Ignored: PE12, PE13, PE14 (Display)", false, SystemDrawing.Color.Yellow);
                    this.UpdateStatusText("* Do NOT forget to test external", false, SystemDrawing.Color.Yellow);
                    this.UpdateStatusText("* power *", false, SystemDrawing.Color.Yellow);
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

        private bool DoTestLeds() {
            var gpioController = GpioController.GetDefault();
            var redLed = gpioController.OpenPin(SC20100.GpioPin.PB0);
            var greenLed = gpioController.OpenPin(SC20100.GpioPin.PE11);

            redLed.SetDriveMode(GpioPinDriveMode.Output);
            greenLed.SetDriveMode(GpioPinDriveMode.Output);

            this.UpdateStatusText("Testing red and green leds.", true);
            this.UpdateStatusText("- The test passes if red and green", false);
            this.UpdateStatusText("  led are blinking.", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("- Only press Next when both leds", false, SystemDrawing.Color.Yellow);
            this.UpdateStatusText("  are blinking.", false, SystemDrawing.Color.Yellow);

            while (!this.doNext && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                greenLed.Write(greenLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                Thread.Sleep(100);
            }

            redLed.Dispose();
            greenLed.Dispose();
            return true;
        }

        private bool DoTestExternalFlash() {
            var storeController = StorageController.FromName(SC20100.StorageController.QuadSpi);
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

        private bool DoTestBuzzer() {
            this.UpdateStatusText("Testing buzzer...", true);

            using (var pwmController3 = PwmController.FromName(SC20100.Timer.Pwm.Controller3.Id)) {
                var pwmPinPB1 = pwmController3.OpenChannel(SC20100.Timer.Pwm.Controller3.PB1);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.PlayTone(pwmController3, pwmPinPB1, 500,  "Generate Pwm 500Hz...");
                this.PlayTone(pwmController3, pwmPinPB1, 1000, "Generate Pwm 1000Hz...");
                this.PlayTone(pwmController3, pwmPinPB1, 2000, "Generate Pwm 2000Hz...");

                pwmPinPB1.Dispose();
            }

            this.UpdateStatusText("Test passes if you heard three", false, SystemDrawing.Color.Yellow);
            this.UpdateStatusText("different tones!", false, SystemDrawing.Color.Yellow);

            while (!this.doNext && this.isRunning)
                Thread.Sleep(100);

            return true;
        }

        private void PlayTone(PwmController controller, PwmChannel channel, int frequency, string status) {
            this.UpdateStatusText(status, false);
            controller.SetDesiredFrequency(frequency);
            channel.Start();
            Thread.Sleep(1000);
            channel.Stop();
        }

        private bool DoTestUsbHost() {
            this.UpdateStatusText("Waiting for usb host initialize...", true);

            UsbWindow.InitializeUsbHostController();
            while (!UsbWindow.IsUsbHostConnected && this.isRunning)
                Thread.Sleep(100);

            if (!this.isRunning)
                return false;

            var storageController = StorageController.FromName(SC20100.StorageController.UsbHostMassStorage);
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
            this.UpdateStatusText("Waiting for Sd initialize...", true);

            var storageController = StorageController.FromName(SC20100.StorageController.SdCard);
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
                        this.UpdateStatusText("Sd: " + BadConnect, true);
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
                    this.UpdateStatusText("Try Rtc again...", true);
                }
            }

            return false;
        }

        private bool DoTestUart() {
            this.UpdateStatusText("Testing VCOM Uart1.", true);
            this.UpdateStatusText("- Connect VCOM Uart1 to PC.", false);
            this.UpdateStatusText("- Open Tera Term. 9600 8N1.", false);
            this.UpdateStatusText("- The test is waiting for any", false);
            this.UpdateStatusText("  character on Tera Term.", false);

            var result = false;

            using (var uart1 = UartController.FromName(SC20100.UartPort.Uart1)) {
                uart1.SetActiveSettings(new UartSetting { BaudRate = 9600 });
                uart1.Enable();

                while (!this.doNext && this.isRunning) {
                    if (uart1.BytesToRead == 0) {
                        Thread.Sleep(100);
                        uart1.Write(new byte[] { (byte)'a' });
                        continue;
                    }

                    var byteToRead = uart1.BytesToRead > uart1.ReadBufferSize ? uart1.ReadBufferSize : uart1.BytesToRead;
                    var read = new byte[byteToRead];
                    uart1.Read(read);

                    for (var i = 0; i < read.Length; i++) {
                        if (read[i] == 'a') {
                            result = true;
                            break;
                        }
                    }

                    if (result) break;
                }
            }

            return result;
        }

        private bool DoTestCan() {
            this.UpdateStatusText("Testing CAN1.", true);
            this.UpdateStatusText("- Open PCAN-View application.", false);
            this.UpdateStatusText("- Nominal speed: 250Kbit/s.", false);
            this.UpdateStatusText("- Data speed: 500Kbit/s.", false);
            this.UpdateStatusText("- The test is waiting for any", false);
            this.UpdateStatusText("  msg with arbitrationId 0x1234.", false);

            var canController = CanController.FromName(SC20100.CanBus.Can1);
            canController.SetNominalBitTiming(new CanBitTiming(15 + 8, 8, 6, 8, false)); // 250 Kbit/s
            canController.SetDataBitTiming(new CanBitTiming(15 + 8, 8, 3, 8, false));    // 500 Kbit/s
            canController.Enable();

            var message = new CanMessage {
                ArbitrationId = 0x1234,
                ExtendedId = true,
                FdCan = false,
                BitRateSwitch = false,
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
            return result;
        }

        private void DoTestGpio() {
            var pinsDefs = new[] {
                SC20100.GpioPin.PE4, SC20100.GpioPin.PC2, SC20100.GpioPin.PC3, SC20100.GpioPin.PA5,
                SC20100.GpioPin.PA6, SC20100.GpioPin.PA7, SC20100.GpioPin.PE8, SC20100.GpioPin.PE7,
                SC20100.GpioPin.PE10, SC20100.GpioPin.PE9, SC20100.GpioPin.PB10, SC20100.GpioPin.PB11,
                SC20100.GpioPin.PB13, SC20100.GpioPin.PB12, SC20100.GpioPin.PA13,
                SC20100.GpioPin.PE1, SC20100.GpioPin.PE0, SC20100.GpioPin.PB8, SC20100.GpioPin.PB9,
                SC20100.GpioPin.PC13, SC20100.GpioPin.PA0, SC20100.GpioPin.PA1, SC20100.GpioPin.PA2,
                SC20100.GpioPin.PA4, SC20100.GpioPin.PC6, SC20100.GpioPin.PC7, SC20100.GpioPin.PE6,
                SC20100.GpioPin.PB3, SC20100.GpioPin.PB4, SC20100.GpioPin.PB5, SC20100.GpioPin.PA14,
                SC20100.GpioPin.PC0, SC20100.GpioPin.PD4, SC20100.GpioPin.PD3, SC20100.GpioPin.PE5,
                SC20100.GpioPin.PC5, SC20100.GpioPin.PD6, SC20100.GpioPin.PD5,
                SC20100.GpioPin.PC1, SC20100.GpioPin.PD15, SC20100.GpioPin.PD14, SC20100.GpioPin.PA3,
                SC20100.GpioPin.PA8, SC20100.GpioPin.PD9, SC20100.GpioPin.PD8,
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
