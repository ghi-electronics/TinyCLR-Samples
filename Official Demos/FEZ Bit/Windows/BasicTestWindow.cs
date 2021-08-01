using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;
using GHIElectronics.TinyCLR.Drivers.STMicroelectronics.LIS2HH12;
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
        private const string Instruction2 = " - User led / buttons";
        private const string Instruction3 = " - Buzzer";
        private const string Instruction4 = " - LIS2HH12 Sensor";
        private const string Instruction5 = " - Wifi";
        private const string Instruction6 = " - Micro Sd";
        private const string Instruction7 = " - RTC crystal";
        private const string Instruction8 = " ";
        private const string Instruction9 = " Press Next when you are ready.";

        private const string MountSuccess = "Mounted successful.";
        private const string BadConnect1 = "Bad device or no connect.";

        private Font font;

        private bool isRunning;

        private TextFlow textFlow;

        private bool doNext = false;

        private bool doTestWifiPassed = false;

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
            // To stop or free, uinitialize variable resource

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
                if (this.isRunning == true && this.DoTestButtons() == true) {
                    this.doNext = false;
                    if (this.isRunning == true && this.DoTestBuzzer() == true) {
                        this.doNext = false;
                        if (this.isRunning == true && this.DoTestI2c() == true) {
                            this.doNext = false;
                            if (this.isRunning == true && this.DoTestWifi() == true) {
                                this.doNext = false;
                                if (this.isRunning == true && this.DoTestSdcard() == true) {
                                    this.doNext = false;
                                    if (this.isRunning == true && this.DoTestRtc() == true) {
                                        this.doNext = false;

                                        this.UpdateStatusText("Last step is Gpio testing.", true);
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

                                            this.DoTestGpio();
                                        }

                                        while (this.doNext == false && this.isRunning == true) {
                                            Thread.Sleep(10);
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

        private bool DoTestLeds() {
            var gpioController = GpioController.GetDefault();

            var greenLed = gpioController.OpenPin(SC20100.GpioPin.PE11);


            greenLed.SetDriveMode(GpioPinDriveMode.Output);


            this.UpdateStatusText("Testing user led.", true);
            this.UpdateStatusText("- The test is passed if user led", false);
            this.UpdateStatusText("  is blinking.", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("- Only press Next button the led", false, System.Drawing.Color.Yellow);
            this.UpdateStatusText("  led is blinking.", false, System.Drawing.Color.Yellow);

            while (this.doNext == false && this.isRunning) {

                greenLed.Write(greenLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

                Thread.Sleep(100);
            }

            greenLed.Dispose();

            return true;
        }

        private bool DoTestButtons() {
            // Need to close all button pins
            // No need to test left and right because we always use them.

            Input.Button.DeinitializeButtons();

            var gpioController = GpioController.GetDefault();

            var buttonDown = gpioController.OpenPin(SC20100.GpioPin.PA1);
            var buttonUp = gpioController.OpenPin(SC20100.GpioPin.PE4);
            var buttonA = gpioController.OpenPin(SC20100.GpioPin.PE5);
            var buttonB = gpioController.OpenPin(SC20100.GpioPin.PE6);

            buttonDown.SetDriveMode(GpioPinDriveMode.InputPullUp);
            buttonUp.SetDriveMode(GpioPinDriveMode.InputPullUp);
            buttonA.SetDriveMode(GpioPinDriveMode.InputPullUp);
            buttonB.SetDriveMode(GpioPinDriveMode.InputPullUp);

            this.UpdateStatusText("Testing buttons.", true);


            this.UpdateStatusText("Wait for press UP button ", false);
            while (buttonUp.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (buttonUp.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            this.UpdateStatusText("Wait for press Down button ", false);
            while (buttonDown.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (buttonDown.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            this.UpdateStatusText("Wait for press A button ", false);
            while (buttonA.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (buttonA.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            this.UpdateStatusText("Wait for press B button ", false);
            while (buttonB.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (buttonB.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            buttonUp.Dispose();
            buttonDown.Dispose();
            buttonA.Dispose();
            buttonB.Dispose();

            // Register button for Input again
            Input.Button.InitializeButtons();

            return true;
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

        private bool DoTestI2c() {

            this.UpdateStatusText("Reading LIS2HH12 sensor...", true);

            try {

                var i2cController = I2cController.FromName("GHIElectronics.TinyCLR.NativeApis.STM32H7.I2cController\\0");
                var lis2hh12 = new LIS2HH12Controller(i2cController);

                while (this.isRunning) {
                    var x = (int)lis2hh12.X;
                    var y = (int)lis2hh12.Y;
                    var z = (int)lis2hh12.Z;

                    if (x != y &&
                     x != z &&
                     y != z &&
                     x != 0 &&
                     y != 0 &&
                     z != 0 &&
                     x != double.MaxValue &&
                     y != double.MaxValue &&
                     z != double.MaxValue) {
                        break;
                    }

                    Thread.Sleep(20);
                }



            }
            catch {

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

        private bool DoTestWifi() {

            this.UpdateStatusText("Checking wifi firmware...", true);

            if (this.doTestWifiPassed)
                return true;

            var gpioController = GpioController.GetDefault();

            var resetPin = gpioController.OpenPin(SC20260.GpioPin.PB13);
            var csPin = gpioController.OpenPin(SC20260.GpioPin.PD15);
            var intPin = gpioController.OpenPin(SC20260.GpioPin.PB12);
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

        private void DoTestGpio() {
            var pinsDefs = new int[] {SC20100.GpioPin.PC6, SC20100.GpioPin.PC7 ,SC20100.GpioPin.PA0,SC20100.GpioPin.PB0,SC20100.GpioPin.PA4,SC20100.GpioPin.PD13,
                               SC20100.GpioPin.PD12,SC20100.GpioPin.PD11,SC20100.GpioPin.PE8,SC20100.GpioPin.PC3,SC20100.GpioPin.PC0,SC20100.GpioPin.PD1,
                               SC20100.GpioPin.PD0,SC20100.GpioPin.PA5,SC20100.GpioPin.PA6,SC20100.GpioPin.PA7,SC20100.GpioPin.PE7
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
