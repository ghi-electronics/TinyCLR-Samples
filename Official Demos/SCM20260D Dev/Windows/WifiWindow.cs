using System;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Shapes;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public class WifiWindow : ApplicationWindow {

        private Canvas canvas;

        private Font font;

        private Text ipAddressLable;
        private Text gatewayLabel;
        private Text subnetmaskLabel;
        private Text dnsLable1;
        private Text dnsLable2;
        private Text selectMikroBus;
        private Text status;

        private Line line;

        private Text ssidLabel;
        private Text passwordLable;
        private TextBox ssid;
        private TextBox password;

        private Button connectButton;

        private bool isWifiConnected;

        private string ipAddress = "IP Address : ";
        private string gateway = "Gateway    : ";
        private string subnetmask = "Subnet Mask: ";
        private string dns1 = "DNS1       : ";
        private string dns2 = "DNS2       : ";

        private NetworkController networkController;

        private GpioPin resetPin;
        private GpioPin csPin;
        private GpioPin intPin;
        private GpioPin enPin;

        private bool isRunning;

        private SpiNetworkCommunicationInterfaceSettings networkCommunicationInterfaceSettings;

        public WifiWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.networkCommunicationInterfaceSettings = new SpiNetworkCommunicationInterfaceSettings();

            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.ipAddressLable = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.ipAddress) {
                ForeColor = Colors.White,
            };

            this.gatewayLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.gateway) {
                ForeColor = Colors.White,
            };

            this.subnetmaskLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.subnetmask) {
                ForeColor = Colors.White,
            };

            this.dnsLable1 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.dns1) {
                ForeColor = Colors.White,
            };

            this.dnsLable2 = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, this.dns2) {
                ForeColor = Colors.White,
            };

            this.selectMikroBus = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Connect wifi click to MikroBus 1") {
                ForeColor = Colors.White,
            };

            this.status = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, " ") {
                ForeColor = Colors.White,
            };

            this.ssidLabel = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "SSID:") {
                ForeColor = Colors.White,
                Width = 80,
            };

            this.passwordLable = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Password:") {
                ForeColor = Colors.White,
                Width = 80,
            };


            this.ssid = new GHIElectronics.TinyCLR.UI.Controls.TextBox() {
                Font = font,
                Width = 120,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.password = new GHIElectronics.TinyCLR.UI.Controls.TextBox() {
                Font = font,
                Width = 120,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };


            this.line = new GHIElectronics.TinyCLR.UI.Shapes.Line(0, 180) {
                Stroke = new GHIElectronics.TinyCLR.UI.Media.Pen(Colors.Yellow)
            };


            this.connectButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Connect") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.connectButton.Click += this.ConnectButton_Click;

            OnScreenKeyboard.Font = this.font;

            this.networkController = NetworkController.FromName("GHIElectronics.TinyCLR.NativeApis.ATWINC15xx.NetworkController");

            //this.isWifiConnected = false;

            this.isRunning = false;
        }


        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {

                if (this.isRunning == false && this.isWifiConnected == false) {
                    this.status.TextContent = "Please wait...";

                    this.status.Invalidate();

                    new Thread(this.CreateEthernet).Start();
                }
                else if (this.isWifiConnected) {
                    this.status.TextContent = "Wifi is already connected.";

                    this.status.Invalidate();
                }
            }
        }


        private void CreateWindow() {

            var startX = 20;
            var startY1 = 40;
            var startY2 = 40;
            var offsetY = 30;

            this.canvas.Children.Clear();

            Canvas.SetLeft(this.ipAddressLable, startX + this.Width / 2); Canvas.SetTop(this.ipAddressLable, startY1); startY1 += offsetY;
            Canvas.SetLeft(this.gatewayLabel, startX + this.Width / 2); Canvas.SetTop(this.gatewayLabel, startY1); startY1 += offsetY;
            Canvas.SetLeft(this.subnetmaskLabel, startX + this.Width / 2); Canvas.SetTop(this.subnetmaskLabel, startY1); startY1 += offsetY;
            Canvas.SetLeft(this.dnsLable1, startX + this.Width / 2); Canvas.SetTop(this.dnsLable1, startY1); startY1 += offsetY;
            Canvas.SetLeft(this.dnsLable2, startX + this.Width / 2); Canvas.SetTop(this.dnsLable2, startY1); startY1 += offsetY;

            this.canvas.Children.Add(this.ipAddressLable);
            this.canvas.Children.Add(this.gatewayLabel);
            this.canvas.Children.Add(this.subnetmaskLabel);
            this.canvas.Children.Add(this.dnsLable1);
            this.canvas.Children.Add(this.dnsLable2);

            Canvas.SetLeft(this.line, startX + this.Width / 2 - 10); Canvas.SetTop(this.line, startY2);
            this.canvas.Children.Add(this.line);

            Canvas.SetLeft(this.selectMikroBus, 5); Canvas.SetTop(this.selectMikroBus, startY2); startY2 += (offsetY);
            this.canvas.Children.Add(this.selectMikroBus);

            Canvas.SetLeft(this.ssidLabel, startX); Canvas.SetTop(this.ssidLabel, startY2);
            this.canvas.Children.Add(this.ssidLabel);

            Canvas.SetLeft(this.ssid, startX + this.ssidLabel.Width); Canvas.SetTop(this.ssid, startY2); startY2 += offsetY;
            this.canvas.Children.Add(this.ssid);

            Canvas.SetLeft(this.passwordLable, startX); Canvas.SetTop(this.passwordLable, startY2);
            this.canvas.Children.Add(this.passwordLable);

            Canvas.SetLeft(this.password, startX + this.passwordLable.Width); Canvas.SetTop(this.password, startY2); startY2 += offsetY;
            this.canvas.Children.Add(this.password);

            Canvas.SetLeft(this.connectButton, this.Width / 2 - this.Width / 4); Canvas.SetTop(this.connectButton, startY2); startY2 += offsetY;
            this.canvas.Children.Add(this.connectButton);

            this.status.TextContent = " ";

            Canvas.SetLeft(this.status, this.Width / 2 - this.Width / 4); Canvas.SetTop(this.status, this.Height - 40);
            this.canvas.Children.Add(this.status);

            // Enable TopBar
            Canvas.SetLeft(this.TopBar, 0); Canvas.SetTop(this.TopBar, 0);
            this.canvas.Children.Add(this.TopBar);

        }

        private void CreateEthernet() {

            this.isRunning = true;
            // MikroBus 1
            var gpioController = GpioController.GetDefault();

            var start = DateTime.Now;

            try {
                Thread.Sleep(100);

                this.resetPin = gpioController.OpenPin(SC20260.GpioPin.PI8);
                this.csPin = gpioController.OpenPin(SC20260.GpioPin.PG12);
                this.intPin = gpioController.OpenPin(SC20260.GpioPin.PG6);
                this.enPin = gpioController.OpenPin(SC20260.GpioPin.PI0);

                this.enPin.SetDriveMode(GpioPinDriveMode.Output);
                this.resetPin.SetDriveMode(GpioPinDriveMode.Output);

                this.enPin.Write(GpioPinValue.Low);
                this.resetPin.Write(GpioPinValue.Low);
                Thread.Sleep(100);

                this.enPin.Write(GpioPinValue.High);
                this.resetPin.Write(GpioPinValue.High);



                var settings = new GHIElectronics.TinyCLR.Devices.Spi.SpiConnectionSettings() {
                    ChipSelectLine = this.csPin,
                    ClockFrequency = 4000000,
                    Mode = GHIElectronics.TinyCLR.Devices.Spi.SpiMode.Mode0,
                    ChipSelectType = GHIElectronics.TinyCLR.Devices.Spi.SpiChipSelectType.Gpio,
                    ChipSelectHoldTime = TimeSpan.FromTicks(10),
                    ChipSelectSetupTime = TimeSpan.FromTicks(10)
                };

                this.networkCommunicationInterfaceSettings.SpiApiName = SC20260.SpiBus.Spi3;
                this.networkCommunicationInterfaceSettings.GpioApiName = "GHIElectronics.TinyCLR.NativeApis.STM32H7.GpioController\\0";
                this.networkCommunicationInterfaceSettings.SpiSettings = settings;
                this.networkCommunicationInterfaceSettings.InterruptPin = this.intPin;
                this.networkCommunicationInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
                this.networkCommunicationInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
                this.networkCommunicationInterfaceSettings.ResetPin = this.resetPin;
                this.networkCommunicationInterfaceSettings.ResetActiveState = GpioPinValue.Low;

                var networkInterfaceSetting = new WiFiNetworkInterfaceSettings() {
                    Ssid = this.ssid.Text,
                    Password = this.password.Text,
                };

                networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 122 });
                networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
                networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
                networkInterfaceSetting.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 75, 75, 75, 75 }), new IPAddress(new byte[] { 75, 75, 75, 76 }) };

                networkInterfaceSetting.MacAddress = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
                networkInterfaceSetting.DhcpEnable = true;
                networkInterfaceSetting.DynamicDnsEnable = true;

                this.networkController.SetInterfaceSettings(networkInterfaceSetting);
                this.networkController.SetCommunicationInterfaceSettings(this.networkCommunicationInterfaceSettings);
                this.networkController.SetAsDefaultController();

                this.networkController.NetworkAddressChanged += this.NetworkController_NetworkAddressChanged;
                this.networkController.NetworkLinkConnectedChanged += this.NetworkController_NetworkLinkConnectedChanged;

                var firmware = Winc15x0Interface.GetFirmwareVersion();


                if (firmware.IndexOf("255.255.255.65535") == 0) {

                    this.resetPin.Dispose();
                    this.csPin.Dispose();
                    this.intPin.Dispose();
                    this.enPin.Dispose();

                    Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {

                        this.canvas.Children.Remove(this.status);

                        Canvas.SetLeft(this.status, this.Width / 2 - this.Width / 4 - this.Width / 8 - this.Width / 16); Canvas.SetTop(this.status, this.Height - 40);
                        this.canvas.Children.Add(this.status);

                        this.status.TextContent = "Please reset application and type ssid, password correctly!";


                        this.status.Invalidate();
                        return null;

                    }, null);

                    goto _return;

                }


                this.networkController.Enable();

            }
            catch {


            }


            while ((DateTime.Now - start).TotalSeconds < 20) {
                if (this.isWifiConnected)
                    break;

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {


                    this.status.TextContent = "Please wait..." + (int)((DateTime.Now - start).TotalSeconds) + " / 20";


                    this.status.Invalidate();
                    return null;

                }, null);

                Thread.Sleep(1000);
            }

            if (this.isWifiConnected == false) {
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {


                    this.status.TextContent = "Wifi connection failed.";


                    this.status.Invalidate();
                    return null;

                }, null);


                this.resetPin.Dispose();
                this.csPin.Dispose();
                this.intPin.Dispose();
                this.enPin.Dispose();

                //var gpioControllerApi = new GpioControllerApiWrapper(NativeApi.Find(NativeApi.GetDefaultName(NativeApiType.GpioController), NativeApiType.GpioController));

                //gpioControllerApi.ClosePin(SC20260.GpioPin.PB3);
                //gpioControllerApi.ClosePin(SC20260.GpioPin.PB4);
                //gpioControllerApi.ClosePin(SC20260.GpioPin.PB5);

                //gpioController.Dispose();
            }
_return:
            this.isRunning = false;
        }

        private void NetworkController_NetworkLinkConnectedChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e) {
            //throw new NotImplementedException();
        }

        private void NetworkController_NetworkAddressChanged(NetworkController sender, NetworkAddressChangedEventArgs e) {
            var ipProperties = sender.GetIPProperties();

            var address = ipProperties.Address.GetAddressBytes();
            var subnet = ipProperties.SubnetMask.GetAddressBytes();
            var gw = ipProperties.GatewayAddress.GetAddressBytes();

            var interfaceProperties = sender.GetInterfaceProperties();

            var dnsCount = ipProperties.DnsAddresses.Length;
            var dns1 = string.Empty;
            var dns2 = string.Empty;

            for (var i = 0; i < dnsCount; i++) {
                var dns = ipProperties.DnsAddresses[i].GetAddressBytes();

                if (i == 0)
                    dns1 = "dns[" + i + "] :" + dns[0] + "." + dns[1] + "." + dns[2] + "." + dns[3];
                else
                    dns2 = "dns[" + i + "] :" + dns[0] + "." + dns[1] + "." + dns[2] + "." + dns[3];
            }

            var ip = address[0] + "." + address[1] + "." + address[2] + "." + address[3];
            var gateway = gw[0] + "." + gw[1] + "." + gw[2] + "." + gw[3];
            var subnetmask = subnet[0] + "." + subnet[1] + "." + subnet[2] + "." + subnet[3];

            this.isWifiConnected = address[0] != 0;

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                this.ipAddressLable.TextContent = this.ipAddress + ip;
                this.gatewayLabel.TextContent = this.gateway + gateway;
                this.subnetmaskLabel.TextContent = this.subnetmask + subnetmask;
                this.dnsLable1.TextContent = this.dns1 + dns1;
                this.dnsLable2.TextContent = this.dns2 + dns2;

                this.status.TextContent = address[0] != 0 ? "Wifi connected." : "Wifi connection failed.";

                this.ipAddressLable.Invalidate();
                this.gatewayLabel.Invalidate();
                this.subnetmaskLabel.Invalidate();
                this.dnsLable1.Invalidate();
                this.dnsLable2.Invalidate();
                this.status.Invalidate();
                return null;

            }, null);

        }


        protected override void Active() {
            // To initialize, reset your variable, design...
            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.CreateWindow();
        }

        protected override void Deactive() =>
            // To stop or free, uinitialize variable resource

            //this.networkController.Disable();

            this.canvas.Children.Clear();
    }
}
