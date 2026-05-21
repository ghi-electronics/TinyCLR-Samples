using System;
using System.Drawing;
using System.Net;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Shapes;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class WifiWindow : ApplicationWindow {
        private Canvas canvas;
        private readonly Font font;

        private readonly Text ipAddressLabel;
        private readonly Text gatewayLabel;
        private readonly Text subnetMaskLabel;
        private readonly Text dnsLabel1;
        private readonly Text dnsLabel2;
        private readonly Text selectMikroBus;
        private readonly Text status;
        private readonly Line line;

        private readonly Text ssidLabel;
        private readonly Text passwordLabel;
        private readonly TextBox ssid;
        private readonly TextBox password;

        private readonly Button connectButton;

        private bool isWifiConnected;
        private bool isRunning;

        private const string IpAddressText = "IP Address : ";
        private const string GatewayText = "Gateway    : ";
        private const string SubnetMaskText = "Subnet Mask: ";
        private const string Dns1Text = "DNS1       : ";
        private const string Dns2Text = "DNS2       : ";

        private readonly NetworkController networkController;
        private readonly SpiNetworkCommunicationInterfaceSettings networkCommunicationInterfaceSettings;

        private GpioPin resetPin;
        private GpioPin csPin;
        private GpioPin intPin;
        private GpioPin enPin;

        public WifiWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.networkCommunicationInterfaceSettings = new SpiNetworkCommunicationInterfaceSettings();
            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.ipAddressLabel = new Text(this.font, IpAddressText) { ForeColor = Colors.White };
            this.gatewayLabel = new Text(this.font, GatewayText) { ForeColor = Colors.White };
            this.subnetMaskLabel = new Text(this.font, SubnetMaskText) { ForeColor = Colors.White };
            this.dnsLabel1 = new Text(this.font, Dns1Text) { ForeColor = Colors.White };
            this.dnsLabel2 = new Text(this.font, Dns2Text) { ForeColor = Colors.White };
            this.selectMikroBus = new Text(this.font, "Connect wifi click to MikroBus 1") { ForeColor = Colors.White };
            this.status = new Text(this.font, " ") { ForeColor = Colors.White };

            this.ssidLabel = new Text(this.font, "SSID:") { ForeColor = Colors.White, Width = 80 };
            this.passwordLabel = new Text(this.font, "Password:") { ForeColor = Colors.White, Width = 80 };

            this.ssid = new TextBox {
                Font = this.font,
                Width = 120,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.password = new TextBox {
                Font = this.font,
                Width = 120,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.line = new Line(0, 180) { Stroke = new GHIElectronics.TinyCLR.UI.Media.Pen(Colors.Yellow) };

            this.connectButton = new Button {
                Child = new Text(this.font, "Connect") {
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
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            if (!this.isRunning && !this.isWifiConnected) {
                this.status.TextContent = "Please wait...";
                this.status.Invalidate();
                new Thread(this.CreateEthernet).Start();
            }
            else if (this.isWifiConnected) {
                this.status.TextContent = "Wifi is already connected.";
                this.status.Invalidate();
            }
        }

        private void CreateWindow() {
            const int startX = 20;
            const int offsetY = 30;
            var startY1 = 40;
            var startY2 = 40;
            var halfWidth = this.Width / 2;

            this.canvas.Children.Clear();

            Canvas.SetLeft(this.ipAddressLabel, startX + halfWidth); Canvas.SetTop(this.ipAddressLabel, startY1); startY1 += offsetY;
            Canvas.SetLeft(this.gatewayLabel, startX + halfWidth);   Canvas.SetTop(this.gatewayLabel, startY1);   startY1 += offsetY;
            Canvas.SetLeft(this.subnetMaskLabel, startX + halfWidth); Canvas.SetTop(this.subnetMaskLabel, startY1); startY1 += offsetY;
            Canvas.SetLeft(this.dnsLabel1, startX + halfWidth);       Canvas.SetTop(this.dnsLabel1, startY1);       startY1 += offsetY;
            Canvas.SetLeft(this.dnsLabel2, startX + halfWidth);       Canvas.SetTop(this.dnsLabel2, startY1);

            this.canvas.Children.Add(this.ipAddressLabel);
            this.canvas.Children.Add(this.gatewayLabel);
            this.canvas.Children.Add(this.subnetMaskLabel);
            this.canvas.Children.Add(this.dnsLabel1);
            this.canvas.Children.Add(this.dnsLabel2);

            Canvas.SetLeft(this.line, startX + halfWidth - 10); Canvas.SetTop(this.line, startY2);
            this.canvas.Children.Add(this.line);

            Canvas.SetLeft(this.selectMikroBus, 5); Canvas.SetTop(this.selectMikroBus, startY2); startY2 += offsetY;
            this.canvas.Children.Add(this.selectMikroBus);

            Canvas.SetLeft(this.ssidLabel, startX); Canvas.SetTop(this.ssidLabel, startY2);
            this.canvas.Children.Add(this.ssidLabel);

            Canvas.SetLeft(this.ssid, startX + this.ssidLabel.Width); Canvas.SetTop(this.ssid, startY2); startY2 += offsetY;
            this.canvas.Children.Add(this.ssid);

            Canvas.SetLeft(this.passwordLabel, startX); Canvas.SetTop(this.passwordLabel, startY2);
            this.canvas.Children.Add(this.passwordLabel);

            Canvas.SetLeft(this.password, startX + this.passwordLabel.Width); Canvas.SetTop(this.password, startY2); startY2 += offsetY;
            this.canvas.Children.Add(this.password);

            Canvas.SetLeft(this.connectButton, halfWidth - this.Width / 4); Canvas.SetTop(this.connectButton, startY2);
            this.canvas.Children.Add(this.connectButton);

            this.status.TextContent = " ";
            Canvas.SetLeft(this.status, halfWidth - this.Width / 4); Canvas.SetTop(this.status, this.Height - 40);
            this.canvas.Children.Add(this.status);

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

                var settings = new SpiConnectionSettings {
                    ChipSelectLine = this.csPin,
                    ClockFrequency = 4000000,
                    Mode = SpiMode.Mode0,
                    ChipSelectType = SpiChipSelectType.Gpio,
                    ChipSelectHoldTime = TimeSpan.FromTicks(10),
                    ChipSelectSetupTime = TimeSpan.FromTicks(10),
                };

                this.networkCommunicationInterfaceSettings.SpiApiName = SC20260.SpiBus.Spi3;
                this.networkCommunicationInterfaceSettings.GpioApiName = "GHIElectronics.TinyCLR.NativeApis.STM32H7.GpioController\\0";
                this.networkCommunicationInterfaceSettings.SpiSettings = settings;
                this.networkCommunicationInterfaceSettings.InterruptPin = this.intPin;
                this.networkCommunicationInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
                this.networkCommunicationInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
                this.networkCommunicationInterfaceSettings.ResetPin = this.resetPin;
                this.networkCommunicationInterfaceSettings.ResetActiveState = GpioPinValue.Low;

                var networkInterfaceSetting = new WiFiNetworkInterfaceSettings {
                    Ssid = this.ssid.Text,
                    Password = this.password.Text,
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

                this.networkController.SetInterfaceSettings(networkInterfaceSetting);
                this.networkController.SetCommunicationInterfaceSettings(this.networkCommunicationInterfaceSettings);
                this.networkController.SetAsDefaultController();

                this.networkController.NetworkAddressChanged += this.NetworkController_NetworkAddressChanged;
                this.networkController.NetworkLinkConnectedChanged += this.NetworkController_NetworkLinkConnectedChanged;

                var firmware = Winc15x0Interface.GetFirmwareVersion();
                if (firmware.IndexOf("255.255.255.65535") == 0) {
                    this.DisposeWincPins();
                    Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {
                        this.canvas.Children.Remove(this.status);
                        Canvas.SetLeft(this.status, this.Width / 2 - this.Width / 4 - this.Width / 8 - this.Width / 16);
                        Canvas.SetTop(this.status, this.Height - 40);
                        this.canvas.Children.Add(this.status);
                        this.status.TextContent = "Please reset application and type ssid, password correctly!";
                        this.status.Invalidate();
                        return null;
                    }, null);
                    this.isRunning = false;
                    return;
                }

                this.networkController.Enable();
            }
            catch {
                // Initialization failed; the connect loop below will time out and report failure.
            }

            while ((DateTime.Now - start).TotalSeconds < 20) {
                if (this.isWifiConnected)
                    break;

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {
                    this.status.TextContent = "Please wait..." + (int)(DateTime.Now - start).TotalSeconds + " / 20";
                    this.status.Invalidate();
                    return null;
                }, null);

                Thread.Sleep(1000);
            }

            if (!this.isWifiConnected) {
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {
                    this.status.TextContent = "Wifi connection failed.";
                    this.status.Invalidate();
                    return null;
                }, null);

                this.DisposeWincPins();
            }

            this.isRunning = false;
        }

        private void DisposeWincPins() {
            this.resetPin?.Dispose();
            this.csPin?.Dispose();
            this.intPin?.Dispose();
            this.enPin?.Dispose();
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

            var dns1 = string.Empty;
            var dns2 = string.Empty;
            for (var i = 0; i < ipProperties.DnsAddresses.Length; i++) {
                var dns = ipProperties.DnsAddresses[i].GetAddressBytes();
                var formatted = "dns[" + i + "] :" + dns[0] + "." + dns[1] + "." + dns[2] + "." + dns[3];
                if (i == 0) dns1 = formatted;
                else        dns2 = formatted;
            }

            var ip = address[0] + "." + address[1] + "." + address[2] + "." + address[3];
            var gateway = gw[0] + "." + gw[1] + "." + gw[2] + "." + gw[3];
            var subnetmask = subnet[0] + "." + subnet[1] + "." + subnet[2] + "." + subnet[3];

            this.isWifiConnected = address[0] != 0;

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {
                this.ipAddressLabel.TextContent = IpAddressText + ip;
                this.gatewayLabel.TextContent = GatewayText + gateway;
                this.subnetMaskLabel.TextContent = SubnetMaskText + subnetmask;
                this.dnsLabel1.TextContent = Dns1Text + dns1;
                this.dnsLabel2.TextContent = Dns2Text + dns2;
                this.status.TextContent = this.isWifiConnected ? "Wifi connected." : "Wifi connection failed.";

                this.ipAddressLabel.Invalidate();
                this.gatewayLabel.Invalidate();
                this.subnetMaskLabel.Invalidate();
                this.dnsLabel1.Invalidate();
                this.dnsLabel2.Invalidate();
                this.status.Invalidate();
                return null;
            }, null);
        }

        protected override void Active() {
            this.canvas = new Canvas();
            this.Child = this.canvas;
            this.CreateWindow();
        }

        protected override void Deactive() => this.canvas.Children.Clear();
    }
}
