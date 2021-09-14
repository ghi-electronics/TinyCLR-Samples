using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Cryptography;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Uart;
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
        private const string Instruction3 = " - External Flash";
        private const string Instruction4 = " - Buzzer";
        private const string Instruction5 = " - Usb Host/ Micro Sd";
        private const string Instruction6 = " - RTC crystal";
        private const string Instruction7 = " - VCOM Uart1";
        private const string Instruction8 = " - CAN1 FD";

        private const string MountSuccess = "Mounted successful.";
        private const string BadConnect1 = "Bad device or no connect.";

        private Font font;

        private bool isRunning;

        private TextFlow textFlow;



        private bool doNext = false;

        public BasicTestWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {

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

            this.textFlow.TextRuns.Add(Instruction6, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction7, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction8, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
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

            this.isRunning = false;

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
            this.isRunning = false;

            Thread.Sleep(10);

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
                case GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select:
                    if (this.isRunning == false) {
                        new Thread(this.ThreadTest).Start();
                    }
                    else {
                        this.doNext = true;
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
        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) => this.UpdateStatusText(this.textFlow, text, this.font, clearscreen, color);

        private void ThreadTest() {
            this.isRunning = true;
            this.doNext = false;

            if (this.DoTestLeds() == true) {
                this.doNext = false;
                if (this.isRunning == true && this.DoTestExternalFlash() == true) {
                    this.doNext = false;
                    if (this.isRunning == true && this.DoTestBuzzer() == true) {
                        this.doNext = false;
                        if (this.isRunning == true && this.DoTestUsbHost()) {
                            this.doNext = false;
                            if (this.isRunning == true && this.DoTestSdcard() == true) {
                                this.doNext = false;
                                if (this.isRunning == true && this.DoTestRtc() == true) {
                                    this.doNext = false;
                                    var testUart = this.DoTestUart();

                                    this.doNext = false;

                                    var testCan = this.DoTestCan();

                                    this.doNext = false;

                                    this.UpdateStatusText(Instruction7 + (testUart ? ": Passed " : ": Failed"), true, testUart ? System.Drawing.Color.White : System.Drawing.Color.Red);
                                    this.UpdateStatusText(Instruction8 + (testCan ? ": Passed " : ": Failed"), false, testCan ? System.Drawing.Color.White : System.Drawing.Color.Red);
                                    this.UpdateStatusText("Last step is Gpio testing.", false);
                                    this.UpdateStatusText("Warning: This step will toggle all", false, System.Drawing.Color.Yellow);
                                    this.UpdateStatusText("exposed gpio. ", false, System.Drawing.Color.Yellow);
                                    this.UpdateStatusText("Only needed for production test.", false, System.Drawing.Color.Yellow);
                                    this.UpdateStatusText("Next to do gpio test, or", false);
                                    this.UpdateStatusText("Back to return main menu", false);

                                    while (this.doNext == false && this.isRunning == true) {
                                        Thread.Sleep(10);
                                    }

                                    if (this.doNext && this.isRunning == true) {
                                        this.doNext = false;


                                        this.UpdateStatusText("Testing gpio....", true);
                                        this.UpdateStatusText("* Ignored: PE12, PE13, PE14 (Display)", false, System.Drawing.Color.Yellow);
                                        this.UpdateStatusText("* Do NOT forget to test external", false, System.Drawing.Color.Yellow);
                                        this.UpdateStatusText("* power *", false, System.Drawing.Color.Yellow);

                                        this.DoTestGpio();
                                    }

                                }
                            }
                        }
                    }

                }


            }

            this.isRunning = false;
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




        private bool DoTestLeds() {
            var gpioController = GpioController.GetDefault();

            var redLed = gpioController.OpenPin(SC20100.GpioPin.PB0);
            var greenLed = gpioController.OpenPin(SC20100.GpioPin.PE11);

            redLed.SetDriveMode(GpioPinDriveMode.Output);
            greenLed.SetDriveMode(GpioPinDriveMode.Output);


            this.UpdateStatusText("Testing red and green leds.", true);
            this.UpdateStatusText("- The test is passed if red and green", false);
            this.UpdateStatusText("  led are blinking.", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("- Only press Next button both leds", false, System.Drawing.Color.Yellow);
            this.UpdateStatusText("  are blinking.", false, System.Drawing.Color.Yellow);


            while (this.doNext == false && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                greenLed.Write(greenLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

                Thread.Sleep(100);
            }

            redLed.Dispose();
            greenLed.Dispose();

            return true;
        }

        private bool DoTestUsbHost() {

            var result = true;
            this.UpdateStatusText("Waiting for usb host initialize...", true);

            UsbWindow.InitializeUsbHostController();

            while (!UsbWindow.IsUsbHostConnected && this.isRunning) Thread.Sleep(100);

            if (this.isRunning == false)
                return false;

            var storageController = StorageController.FromName(SC20100.StorageController.UsbHostMassStorage);

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

            var storageController = StorageController.FromName(SC20100.StorageController.SdCard);

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

            using (var pwmController3 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20100.Timer.Pwm.Controller3.Id)) {

                var pwmPinPB1 = pwmController3.OpenChannel(SC20100.Timer.Pwm.Controller3.PB1);

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


            }

            this.UpdateStatusText("Testing is success if you heard three", false, System.Drawing.Color.Yellow);
            this.UpdateStatusText("kind of sounds!", false, System.Drawing.Color.Yellow);

            while (this.doNext == false && this.isRunning) {
                Thread.Sleep(100);
            }

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

        private bool DoTestUart() {
            this.UpdateStatusText("Testing VCOM Uart1.", true);

            this.UpdateStatusText("- Connect VCOM Uart1 to PC.", false);
            this.UpdateStatusText("- Open Tera Term. Baudrate: 9600, ", false);
            this.UpdateStatusText("  Data: 8, Parity: None, StopBit: 1.", false);
            this.UpdateStatusText("- The test is waiting any character", false);
            this.UpdateStatusText("  on Tera Term screen.", false);

            var result = false;

            using (var uart1 = UartController.FromName(SC20100.UartPort.Uart1)) {

                var setting = new UartSetting() {
                    BaudRate = 9600
                };

                uart1.SetActiveSettings(setting);
                uart1.Enable();

                var totalReceived = 0;

                while (this.doNext == false && this.isRunning) {

                    if (uart1.BytesToRead == 0) {
                        Thread.Sleep(100);

                        uart1.Write(new byte[] { (byte)('a') });
                        continue;
                    }

                    var byteToRead = uart1.BytesToRead > uart1.ReadBufferSize ? uart1.ReadBufferSize : uart1.BytesToRead;

                    var read = new byte[byteToRead];

                    totalReceived += uart1.Read(read);

                    for (var i = 0; i < read.Length; i++) {
                        if (read[i] == 'a') {
                            result = true;
                            break;
                        }
                    }

                    if (result == true)
                        break;
                }
            }


            return result;
        }

        private bool DoTestCan() {
            this.UpdateStatusText("Testing CAN1.", true);
            this.UpdateStatusText("- Open PCAN-View application.", false);
            this.UpdateStatusText("- Nominal speed: 250Kbit/s.", false);
            this.UpdateStatusText("- Data speed: 500Kbit/.", false);
            this.UpdateStatusText("- The test is waiting for any", false);
            this.UpdateStatusText("  msg with arbitrationId 0x1234.", false);

            var canController = CanController.FromName(SC20100.CanBus.Can1);

            canController.SetNominalBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(15 + 8, 8, 6, 8, false)); // 250Kbit/s          

            canController.SetDataBitTiming(new GHIElectronics.TinyCLR.Devices.Can.CanBitTiming(15 + 8, 8, 3, 8, false)); //500kbit/s 

            canController.Enable();

            var result = false;

            var message = new CanMessage() {
                ArbitrationId = 0x1234,
                ExtendedId = true,
                FdCan = !true,
                BitRateSwitch = !true,
                Data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                Length = 8
            };

            while (this.doNext == false && this.isRunning) {

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

                var msgs = new GHIElectronics.TinyCLR.Devices.Can.CanMessage[canController.MessagesToRead];

                for (var i = 0; i < msgs.Length; i++) {
                    msgs[i] = new GHIElectronics.TinyCLR.Devices.Can.CanMessage();
                }

                for (var i = 0; i < msgs.Length; i++) {
                    canController.ReadMessages(msgs, 0, msgs.Length);

                    if (msgs[i].ArbitrationId == message.ArbitrationId) {
                        this.doNext = true;

                        result = true;
                        break;
                    }
                }

                if (result)
                    break;
            }

            canController.Disable();

            return result;
        }

        private void DoTestGpio() {
            var pinsDefs = new int[] {SC20100.GpioPin.PE4, SC20100.GpioPin.PC2, SC20100.GpioPin.PC3, SC20100.GpioPin.PA5, SC20100.GpioPin.PA6, SC20100.GpioPin.PA7, SC20100.GpioPin.PE8, SC20100.GpioPin.PE7, SC20100.GpioPin.PE10, SC20100.GpioPin.PE9, SC20100.GpioPin.PB10, SC20100.GpioPin.PB11, SC20100.GpioPin.PB13, SC20100.GpioPin.PB12, SC20100.GpioPin.PA13
                , SC20100.GpioPin.PE1, SC20100.GpioPin.PE0, SC20100.GpioPin.PB8, SC20100.GpioPin.PB9, SC20100.GpioPin.PC13, SC20100.GpioPin.PA0, SC20100.GpioPin.PA1, SC20100.GpioPin.PA2, SC20100.GpioPin.PA4, SC20100.GpioPin.PC6, SC20100.GpioPin.PC7, SC20100.GpioPin.PE6, SC20100.GpioPin.PB3, SC20100.GpioPin.PB4, SC20100.GpioPin.PB5, SC20100.GpioPin.PA14
                , SC20100.GpioPin.PC0, SC20100.GpioPin.PD4, SC20100.GpioPin.PD3, SC20100.GpioPin.PE5, SC20100.GpioPin.PC5, SC20100.GpioPin.PD6, SC20100.GpioPin.PD5
                , SC20100.GpioPin.PC1, SC20100.GpioPin.PD15, SC20100.GpioPin.PD14, SC20100.GpioPin.PA3, SC20100.GpioPin.PA8, SC20100.GpioPin.PD9, SC20100.GpioPin.PD8
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
