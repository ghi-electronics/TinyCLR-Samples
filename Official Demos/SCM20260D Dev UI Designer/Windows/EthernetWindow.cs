using System;
using System.Drawing;
using System.Net;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using SystemDrawing = System.Drawing;

namespace Demos {
    public class EthernetWindow : ApplicationWindow {
        private Canvas canvas;
        private Font font;

        private Text ipAddressLabel;
        private Text gatewayLabel;
        private Text subnetMaskLabel;
        private Text dnsLabel1;
        private Text dnsLabel2;

        private const string IpAddressText = "IP Address : ";
        private const string GatewayText = "Gateway    : ";
        private const string SubnetMaskText = "Subnet Mask: ";
        private const string Dns1Text = "DNS1       : ";
        private const string Dns2Text = "DNS2       : ";

        private NetworkController networkController;
        private readonly GpioPin resetPin;

        public EthernetWindow(SystemDrawing.Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            var gpioController = GpioController.GetDefault();
            this.resetPin = gpioController.OpenPin(SC20260.GpioPin.PG3);
            this.resetPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private void CreateWindow() {
            const int startX = 20;
            var startY = 40;
            const int offsetY = 30;

            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.canvas.Children.Clear();

            this.ipAddressLabel = new Text(this.font, IpAddressText) { ForeColor = Colors.White };
            this.gatewayLabel = new Text(this.font, GatewayText) { ForeColor = Colors.White };
            this.subnetMaskLabel = new Text(this.font, SubnetMaskText) { ForeColor = Colors.White };
            this.dnsLabel1 = new Text(this.font, Dns1Text) { ForeColor = Colors.White };
            this.dnsLabel2 = new Text(this.font, Dns2Text) { ForeColor = Colors.White };

            Canvas.SetLeft(this.ipAddressLabel, startX); Canvas.SetTop(this.ipAddressLabel, startY); startY += offsetY;
            Canvas.SetLeft(this.gatewayLabel, startX);   Canvas.SetTop(this.gatewayLabel, startY);   startY += offsetY;
            Canvas.SetLeft(this.subnetMaskLabel, startX); Canvas.SetTop(this.subnetMaskLabel, startY); startY += offsetY;
            Canvas.SetLeft(this.dnsLabel1, startX);      Canvas.SetTop(this.dnsLabel1, startY);      startY += offsetY;
            Canvas.SetLeft(this.dnsLabel2, startX);      Canvas.SetTop(this.dnsLabel2, startY);

            this.canvas.Children.Add(this.ipAddressLabel);
            this.canvas.Children.Add(this.gatewayLabel);
            this.canvas.Children.Add(this.subnetMaskLabel);
            this.canvas.Children.Add(this.dnsLabel1);
            this.canvas.Children.Add(this.dnsLabel2);

            // Top bar.
            Canvas.SetLeft(this.TopBar, 0);
            Canvas.SetTop(this.TopBar, 0);
            this.canvas.Children.Add(this.TopBar);
        }

        private void CreateEthernet() {
            this.resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            this.resetPin.Write(GpioPinValue.High);
            Thread.Sleep(100);

            this.networkController = NetworkController.FromName("GHIElectronics.TinyCLR.NativeApis.STM32H7.EthernetEmacController\\0");

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

            this.networkController.SetInterfaceSettings(networkInterfaceSetting);
            this.networkController.SetCommunicationInterfaceSettings(new BuiltInNetworkCommunicationInterfaceSettings());
            this.networkController.SetAsDefaultController();

            this.networkController.NetworkAddressChanged += this.NetworkController_NetworkAddressChanged;
            this.networkController.NetworkLinkConnectedChanged += this.NetworkController_NetworkLinkConnectedChanged;
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

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {
                this.ipAddressLabel.TextContent = IpAddressText + ip;
                this.gatewayLabel.TextContent = GatewayText + gateway;
                this.subnetMaskLabel.TextContent = SubnetMaskText + subnetmask;
                this.dnsLabel1.TextContent = Dns1Text + dns1;
                this.dnsLabel2.TextContent = Dns2Text + dns2;

                this.ipAddressLabel.Invalidate();
                this.gatewayLabel.Invalidate();
                this.subnetMaskLabel.Invalidate();
                this.dnsLabel1.Invalidate();
                this.dnsLabel2.Invalidate();
                return null;
            }, null);
        }

        protected override void Active() {
            this.canvas = new Canvas();
            this.Child = this.canvas;

            this.CreateWindow();
            this.CreateEthernet();
            this.networkController.Enable();
        }

        protected override void Deactive() {
            this.networkController.Disable();
            this.canvas.Children.Clear();
            this.font.Dispose();
        }
    }
}
