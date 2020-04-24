using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Net;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using FEZ_Portal.Properties;

namespace FEZ_Portal {
    public sealed class SelectPage {
        private Canvas canvas;
        private Font font;
        private TextBox ssidTextBox;
        private TextBox passwordTextBox;
        public UIElement Elements { get; }
        public SelectPage() {
            this.canvas = new Canvas();
            this.font = Resources.GetFont(Resources.FontResources.NinaB);
            this.Elements = this.CreatePage();
            OnScreenKeyboard.Font = this.font;
        }

        private UIElement CreatePage() {

            this.canvas.Children.Clear();

            //Label SSID
            var ssidText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "SSID:") {
                ForeColor = Colors.White,
            };
            Canvas.SetLeft(ssidText, 150);
            Canvas.SetTop(ssidText, 75);
            this.canvas.Children.Add(ssidText);
            //TextBox SSID
            this.ssidTextBox = new TextBox() {
                Text = "",
                Font = font,
                Width = 200,
                Height = 25,
            };
            Canvas.SetLeft(this.ssidTextBox, 150);
            Canvas.SetTop(this.ssidTextBox, 90);
            this.canvas.Children.Add(this.ssidTextBox);

            //Label password
            var passwordText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Password:") {
                ForeColor = Colors.White,
            };
            Canvas.SetLeft(passwordText, 150);
            Canvas.SetTop(passwordText, 130);
            this.canvas.Children.Add(passwordText);
            //TextBox password
            this.passwordTextBox = new TextBox() {
                Text = "",
                Font = font,
                Width = 200,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Canvas.SetLeft(this.passwordTextBox, 150);
            Canvas.SetTop(this.passwordTextBox, 145);
            this.canvas.Children.Add(this.passwordTextBox);
            //Connect Button
            var buttonText = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Connect") {
                ForeColor = Colors.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var connectButton = new Button() {
                Child = buttonText,
                Width = 100,
                Height = 40,
            };
            Canvas.SetLeft(connectButton, 200);
            Canvas.SetTop(connectButton, 220);
            this.canvas.Children.Add(connectButton);

            connectButton.Click += this.ConnectButton_Click;

            return this.canvas;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e) {

            if ("TouchUpEvent".CompareTo(e.RoutedEvent.Name) == 0)
                new Thread(this.ConnectWifi).Start();
        }

        private void ConnectWifi() {
            var enablePin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA8);
            enablePin.SetDriveMode(GpioPinDriveMode.Output);
            enablePin.Write(GpioPinValue.High);

            var networkCommunicationInterfaceSettings =
                new SpiNetworkCommunicationInterfaceSettings();

            var cs = GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault().
                OpenPin(GHIElectronics.TinyCLR.Pins.SC20260.GpioPin.PA6);

            var settings = new GHIElectronics.TinyCLR.Devices.Spi.SpiConnectionSettings() {
                ChipSelectLine = cs,
                ClockFrequency = 4000000,
                Mode = GHIElectronics.TinyCLR.Devices.Spi.SpiMode.Mode0,
                ChipSelectType = GHIElectronics.TinyCLR.Devices.Spi.SpiChipSelectType.Gpio,
                ChipSelectHoldTime = TimeSpan.FromTicks(10),
                ChipSelectSetupTime = TimeSpan.FromTicks(10)
            };

            networkCommunicationInterfaceSettings.SpiApiName = SC20260.SpiBus.Spi3;
            networkCommunicationInterfaceSettings.GpioApiName = SC20260.GpioPin.Id;
            networkCommunicationInterfaceSettings.SpiSettings = settings;
            networkCommunicationInterfaceSettings.InterruptPin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PF10);
            networkCommunicationInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
            networkCommunicationInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
            networkCommunicationInterfaceSettings.ResetPin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PF8);
            networkCommunicationInterfaceSettings.ResetActiveState = GpioPinValue.Low;

            var networkController = NetworkController.FromName
                ("GHIElectronics.TinyCLR.NativeApis.ATWINC15xx.NetworkController");

            var networkInterfaceSetting = new WiFiNetworkInterfaceSettings() {
                Ssid = this.ssidTextBox.Text,
                Password = this.passwordTextBox.Text,
            };

            networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 122 });
            networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
            networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
            networkInterfaceSetting.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 75, 75, 75, 75 }), new IPAddress(new byte[] { 75, 75, 75, 76 }) };
            networkInterfaceSetting.MacAddress = new byte[] { 0x00, 0x4, 0x00, 0x00, 0x00, 0x00 };
            networkInterfaceSetting.IsDhcpEnabled = true;
            networkInterfaceSetting.IsDynamicDnsEnabled = true;
            networkInterfaceSetting.TlsEntropy = new byte[] { 0, 1, 2, 3 };
            networkController.SetInterfaceSettings(networkInterfaceSetting);
            networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);
            networkController.SetAsDefaultController();
            networkController.NetworkAddressChanged += NetworkController_NetworkAddressChanged;
            networkController.NetworkLinkConnectedChanged += NetworkController_NetworkLinkConnectedChanged;
            networkController.Enable();
        }
        public static ConnectPage ConnectPage { get; set; }
        private static void NetworkController_NetworkLinkConnectedChanged
            (NetworkController sender, NetworkLinkConnectedChangedEventArgs e) {
            // Raise event connect/disconnect         
        }
        private static void NetworkController_NetworkAddressChanged
            (NetworkController sender, NetworkAddressChangedEventArgs e) {
            var ipProperties = sender.GetIPProperties();
            var address = ipProperties.Address.GetAddressBytes();
            //Debug.WriteLine("IP: " + address[0] + "." + address[1] + "." + address[2] + "." + address[3]);

            if (address[0] != 0) {
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ => {
                    Program.ConnectPage = new ConnectPage("IP: " + address[0] + "." + address[1] + "." + address[2] + "." + address[3]);
                    Program.WpfWindow.Child = Program.ConnectPage.Elements;
                    Program.WpfWindow.Invalidate();
                    return null;
                }, null);
            }
        }
    }
}
