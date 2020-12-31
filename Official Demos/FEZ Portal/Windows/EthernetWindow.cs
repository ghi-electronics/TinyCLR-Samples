using System;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public class EthernetWindow : ApplicationWindow {

        private Canvas canvas;

        private Font font;

        private Text ipAddressLable;
        private Text gatewayLabel;
        private Text subnetmaskLabel;
        private Text dnsLable1;
        private Text dnsLable2;

        private string ipAddress = "IP Address : ";
        private string gateway = "Gateway    : ";
        private string subnetmask = "Subnet Mask: ";
        private string dns1 = "DNS1       : ";
        private string dns2 = "DNS2       : ";

        private NetworkController networkController;
        private GpioPin resetPin;

        public EthernetWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            var gpioController = GpioController.GetDefault();

            this.resetPin = gpioController.OpenPin(SC20260.GpioPin.PG3);
            this.resetPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private void CreateWindow() {

            var startX = 20;
            var startY = 40;
            var offsetY = 30;

            this.font = Resources.GetFont(Resources.FontResources.droid_reg12);

            this.canvas.Children.Clear();


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

            Canvas.SetLeft(this.ipAddressLable, startX); Canvas.SetTop(this.ipAddressLable, startY); startY += offsetY;
            Canvas.SetLeft(this.gatewayLabel, startX); Canvas.SetTop(this.gatewayLabel, startY); startY += offsetY;
            Canvas.SetLeft(this.subnetmaskLabel, startX); Canvas.SetTop(this.subnetmaskLabel, startY); startY += offsetY;
            Canvas.SetLeft(this.dnsLable1, startX); Canvas.SetTop(this.dnsLable1, startY); startY += offsetY;
            Canvas.SetLeft(this.dnsLable2, startX); Canvas.SetTop(this.dnsLable2, startY); startY += offsetY;

            this.canvas.Children.Add(this.ipAddressLable);
            this.canvas.Children.Add(this.gatewayLabel);
            this.canvas.Children.Add(this.subnetmaskLabel);
            this.canvas.Children.Add(this.dnsLable1);
            this.canvas.Children.Add(this.dnsLable2);

            // Enable TopBar
            Canvas.SetLeft(this.TopBar, 0); Canvas.SetTop(this.TopBar, 0);
            this.canvas.Children.Add(this.TopBar);
        }

        private void CreateEthernet() {

            this.resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);

            this.resetPin.Write(GpioPinValue.High);
            Thread.Sleep(100);

            this.networkController = NetworkController.FromName("GHIElectronics.TinyCLR.NativeApis.STM32H7.EthernetEmacController\\0");

            var networkInterfaceSetting = new EthernetNetworkInterfaceSettings();
            var networkCommunicationInterfaceSettings = new BuiltInNetworkCommunicationInterfaceSettings();

            networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 122 });
            networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
            networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
            networkInterfaceSetting.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 75, 75, 75, 75 }), new IPAddress(new byte[] { 75, 75, 75, 76 }) };

            networkInterfaceSetting.MacAddress = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
            networkInterfaceSetting.DhcpEnable = true;
            networkInterfaceSetting.DynamicDnsEnable = true;

            this.networkController.SetInterfaceSettings(networkInterfaceSetting);
            this.networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);
            this.networkController.SetAsDefaultController();

            this.networkController.NetworkAddressChanged += this.NetworkController_NetworkAddressChanged;
            this.networkController.NetworkLinkConnectedChanged += this.NetworkController_NetworkLinkConnectedChanged;
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

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {

                this.ipAddressLable.TextContent = this.ipAddress + ip;
                this.gatewayLabel.TextContent = this.gateway + gateway;
                this.subnetmaskLabel.TextContent = this.subnetmask + subnetmask;
                this.dnsLable1.TextContent = this.dns1 + dns1;
                this.dnsLable2.TextContent = this.dns2 + dns2;

                this.ipAddressLable.Invalidate();
                this.gatewayLabel.Invalidate();
                this.subnetmaskLabel.Invalidate();
                this.dnsLable1.Invalidate();
                this.dnsLable2.Invalidate();
                return null;

            }, null);

        }


        protected override void Active() {
            // To initialize, reset your variable, design...
            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.CreateWindow();

            this.CreateEthernet();

            this.networkController.Enable();

        }

        protected override void Deactive() {
            // To stop or free, uinitialize variable resource

            this.networkController.Disable();

            this.canvas.Children.Clear();

            this.font.Dispose();


        }
    }
}
