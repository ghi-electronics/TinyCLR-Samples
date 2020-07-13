using System;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class BasicTestWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This step will test:";
        private const string Instruction2 = " - External Ram.";
        private const string Instruction3 = " - External flash.";
        private const string Instruction4 = " - Ethernet.";
        private const string Instruction5 = " ";
        private const string Instruction6 = " ** Please connect ethernet cable for Ethernet testing **";
        private const string Instruction7 = " ** Ethernet test is not available on FEZ Portal **";
        private const string Instruction8 = " Press Test button when you are ready.";


        private Font font;

        private bool isRunning;

        private TextFlow textFlow;

        private Button testButton;

        private bool ethernetConnect = false;


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

            this.testButton.Click += this.TestButton_Click;
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

            var yellowColor = System.Drawing.Color.Yellow;

            this.textFlow.TextRuns.Add(Instruction7, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(yellowColor.R, yellowColor.G, yellowColor.B));
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

        private void ThreadTest() {
            this.isRunning = true;

            var extRamTestResult = this.DoTestExternalRam();
            var extFlashTestResult = this.DoTestExternalFlash();
            var ethernetTestResult = this.DoTestEthernet();

            this.UpdateStatusText("External ram test result: " + (extRamTestResult == true ? "Passed" : "Failed"), true, extRamTestResult == true ? System.Drawing.Color.White : System.Drawing.Color.Red);
            this.UpdateStatusText("External flash test result: " + (extFlashTestResult == true ? "Passed" : "Failed"), false, extFlashTestResult == true ? System.Drawing.Color.White : System.Drawing.Color.Red);
            this.UpdateStatusText("Ethernet test result: " + (ethernetTestResult == true ? "Passed" : "Failed"), false, ethernetTestResult == true ? System.Drawing.Color.White : System.Drawing.Color.Red);

            this.isRunning = false;
        }

        private bool DoTestExternalRam() {
            var result = true;

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
            this.UpdateStatusText("**If you are waiting more than 5 seconds, meaning RAM test is failed.**", false);

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

            return result;

        }

        private bool DoTestExternalFlash() {

            this.isRunning = true;
            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);
            var drive = storeController.Provider;
            var result = true;

            drive.Open();


            var sectorSize = drive.Descriptor.RegionSizes[0];

            var textWrite = System.Text.UTF8Encoding.UTF8.GetBytes("this is for test");

            var dataRead = new byte[sectorSize];
            var dataWrite = new byte[sectorSize];

            for (var i = 0; i < sectorSize; i += textWrite.Length) {
                Array.Copy(textWrite, 0, dataWrite, i, textWrite.Length);
            }

            var roundTest = 0;
            var startSector = 0;
            var endSector = 8;

_again:
            if (roundTest == 1) {
                startSector = 4088;
                endSector = startSector + 8;
            }

            for (var s = startSector; s < endSector; s++) {

                var address = s * sectorSize;
                this.UpdateStatusText("External flash - Erasing sector " + s, true);
                // Erase
                drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                // Read - check for blank
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != 0xFF) {

                        this.UpdateStatusText("External flash - Erase failed at: " + idx, false);
                        result = false;
                        goto _return;
                    }
                }

                // Write
                this.UpdateStatusText("External flash - Writing sector " + s, false);
                drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                this.UpdateStatusText("External flash - Reading sector " + s, false);
                //Read to compare
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != dataWrite[idx]) {

                        this.UpdateStatusText("External flash - Compare failed at: " + idx, false);
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
            this.isRunning = false;

            return result;
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

            var networkInterfaceSetting = new EthernetNetworkInterfaceSettings();
            var networkCommunicationInterfaceSettings = new BuiltInNetworkCommunicationInterfaceSettings();

            networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 122 });
            networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
            networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
            networkInterfaceSetting.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 75, 75, 75, 75 }), new IPAddress(new byte[] { 75, 75, 75, 76 }) };

            networkInterfaceSetting.MacAddress = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
            networkInterfaceSetting.IsDhcpEnabled = true;
            networkInterfaceSetting.IsDynamicDnsEnabled = true;

            networkController.SetInterfaceSettings(networkInterfaceSetting);
            networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);
            networkController.SetAsDefaultController();

            networkController.NetworkAddressChanged += this.NetworkController_NetworkAddressChanged;
            networkController.NetworkLinkConnectedChanged += this.NetworkController_NetworkLinkConnectedChanged;

            this.UpdateStatusText("Test Ethernet is starting, take up to 10 seconds... ", true);

            var start = DateTime.Now;

            networkController.Enable();

            while (this.ethernetConnect == false) {
                var end = DateTime.Now - start;
                if (end.TotalSeconds > 10) break;
            }

            networkController.Disable();
            networkController.Dispose();
            resetPin.Dispose();

            return this.ethernetConnect;
        }

        private void NetworkController_NetworkLinkConnectedChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e) {
            //throw new NotImplementedException();
        }

        private void NetworkController_NetworkAddressChanged(NetworkController sender, NetworkAddressChangedEventArgs e) {
            var ipProperties = sender.GetIPProperties();

            var address = ipProperties.Address.GetAddressBytes();
            var subnet = ipProperties.SubnetMask.GetAddressBytes();
            var gw = ipProperties.GatewayAddress.GetAddressBytes();

            //var interfaceProperties = sender.GetInterfaceProperties();

            //var dnsCount = ipProperties.DnsAddresses.Length;
            //var dns1 = string.Empty;
            //var dns2 = string.Empty;

            //for (var i = 0; i < dnsCount; i++) {
            //    var dns = ipProperties.DnsAddresses[i].GetAddressBytes();

            //    if (i == 0)
            //        dns1 = "dns[" + i + "] :" + dns[0] + "." + dns[1] + "." + dns[2] + "." + dns[3];
            //    else
            //        dns2 = "dns[" + i + "] :" + dns[0] + "." + dns[1] + "." + dns[2] + "." + dns[3];
            //}

            var ip = address[0] + "." + address[1] + "." + address[2] + "." + address[3];
            var gateway = gw[0] + "." + gw[1] + "." + gw[2] + "." + gw[3];
            var subnetmask = subnet[0] + "." + subnet[1] + "." + subnet[2] + "." + subnet[3];

            if (address[0] != 0) {
                this.UpdateStatusText("ip: " + ip, false);
                this.UpdateStatusText("gateway: " + gateway, false);
                this.UpdateStatusText("subnetmask: " + subnetmask, false);
            }

            this.ethernetConnect = address[0] != 0 ? true : false;

        }


        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) {

            var timeout = 100;

            try {

                var count = this.textFlow.TextRuns.Count + 2;

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(timeout), _ => {

                    if (clearscreen)
                        this.textFlow.TextRuns.Clear();

                    this.textFlow.TextRuns.Add(text, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
                    this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                    return null;

                }, null);

                if (clearscreen) {
                    while (this.textFlow.TextRuns.Count < 2) {
                        Thread.Sleep(10);
                    }
                }
                else {
                    while (this.textFlow.TextRuns.Count < count) {
                        Thread.Sleep(10);
                    }
                }
            }
            catch {

            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

        }
    }
}
