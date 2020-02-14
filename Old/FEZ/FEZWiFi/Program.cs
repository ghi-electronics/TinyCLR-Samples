using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInterface;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.STMicroelectronics.SPWF04Sx;
using GHIElectronics.TinyCLR.Pins;

namespace FEZWiFi {
    public static class Program {
        private static GpioPin led1;
        private static GpioPin btn1;
        private static SPWF04SxInterface wifi;

        public static void Main() {
            var cont = GpioController.GetDefault();
            var reset = cont.OpenPin(FEZ.GpioPin.WiFiReset);
            var irq = cont.OpenPin(FEZ.GpioPin.WiFiInterrupt);
            var mode = cont.OpenPin(FEZCLR.GpioPin.PA0);
            var scont = SpiController.FromName(FEZ.SpiBus.WiFi);
            var spi = scont.GetDevice(SPWF04SxInterface.GetConnectionSettings(SpiChipSelectType.Gpio, FEZ.GpioPin.WiFiChipSelect));

            mode.SetDriveMode(GpioPinDriveMode.InputPullDown);

            led1 = cont.OpenPin(FEZ.GpioPin.Led1);
            led1.SetDriveMode(GpioPinDriveMode.Output);

            btn1 = cont.OpenPin(FEZ.GpioPin.Btn1);
            btn1.SetDriveMode(GpioPinDriveMode.InputPullUp);

            wifi = new SPWF04SxInterface(spi, irq, reset);

            wifi.IndicationReceived += (s, e) => Debug.WriteLine($"WIND: {Program.WindToName(e.Indication)} {e.Message}");
            wifi.ErrorReceived += (s, e) => Debug.WriteLine($"ERROR: {e.Error} {e.Message}");

            wifi.TurnOn();

            NetworkInterface.ActiveNetworkInterface = wifi;

            WaitForButton();
            wifi.JoinNetwork("SSID", "password");

            WaitForButton();
            wifi.ClearTlsServerRootCertificate();
            wifi.SetTlsServerRootCertificate(Resources.GetBytes(Resources.BinaryResources.DigiCertGlobalRootCA));

            while (true) {
                WaitForButton();

                //.NET
                TestSocket("www.ghielectronics.com", "/", 443, "ghielectronics.com");

                //WiFi
                //TestHttp("www.ghielectronics.com", "/", 443, SPWF04SxConnectionSecurityType.Tls, true);
                //TestSocket("www.ghielectronics.com", "/", 443, SPWF04SxConnectionType.Tcp, SPWF04SxConnectionSecurityType.Tls, "ghielectronics.com");
            }
        }

        private static void TestSocket(string host, string url, int port, SPWF04SxConnectionType connectionType, SPWF04SxConnectionSecurityType connectionSecurity, string commonName = null) {
            var buffer = new byte[512];
            var id = wifi.OpenSocket(host, port, connectionType, connectionSecurity, commonName);

            var cont = true;
            while (cont) {
                var start = DateTime.UtcNow;

                wifi.WriteSocket(id, Encoding.UTF8.GetBytes($"GET {url} HTTP/1.1\r\nHost: {host}\r\n\r\n"));

                Thread.Sleep(100);

                var total = 0;
                var first = true;
                while ((wifi.QuerySocket(id) is var avail && avail > 0) || first || total < 120) {
                    if (avail > 0) {
                        first = false;

                        var read = wifi.ReadSocket(id, buffer, 0, Math.Min(avail, buffer.Length));

                        total += read;

                        Debugger.Log(0, "", Encoding.UTF8.GetString(buffer, 0, read));
                    }

                    Thread.Sleep(100);
                }

                Debug.WriteLine($"\r\nRead: {total:N0} in {(DateTime.UtcNow - start).TotalMilliseconds:N0}ms");

                WaitForButton();
            }

            wifi.CloseSocket(id);
        }

        private static void TestHttp(string host, string url, int port, SPWF04SxConnectionSecurityType security, bool get) {
            var buffer = new byte[512];
            var start = DateTime.UtcNow;
            var code = get ? wifi.SendHttpGet(host, url, port, security) : wifi.SendHttpPost(host, url, port, security);

            Debug.WriteLine($"HTTP {code}");

            var total = 0;
            while (wifi.ReadHttpResponse(buffer, 0, buffer.Length) is var read && read > 0) {
                total += read;

                try {
                    Debugger.Log(0, "", Encoding.UTF8.GetString(buffer, 0, read));
                }
                catch {
                    Debugger.Log(0, "", Encoding.UTF8.GetString(buffer, 0, read - 1));
                }

                Thread.Sleep(100);
            }

            Debug.WriteLine($"\r\nRead: {total:N0} in {(DateTime.UtcNow - start).TotalMilliseconds:N0}ms");
        }

        private static void TestSocket(string host, string url, int port, string commonName = null) {
            if (commonName != null) {
                wifi.ForceSocketsTls = true;
                wifi.ForceSocketsTlsCommonName = commonName;
            }

            var buffer = new byte[512];
            var data = Encoding.UTF8.GetBytes($"GET {url} HTTP/1.1\r\nHost: {host}\r\n\r\n");
            var entry = Dns.GetHostEntry(host);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(new IPEndPoint(entry.AddressList[0], port));
            socket.ReceiveTimeout = 250;

            var cont = true;
            while (cont) {
                var start = DateTime.UtcNow;
                var written = socket.Send(data);

                Thread.Sleep(100);

                var total = 0;
                var first = true;
                while ((socket.Poll(0, SelectMode.SelectRead) is var ready && ready) || first || total < 120) {
                    if (ready && socket.Receive(buffer) is var read && read > 0) {
                        first = false;

                        Debugger.Log(0, "", Encoding.UTF8.GetString(buffer, 0, read));

                        total += read;
                    }

                    Thread.Sleep(100);
                }

                Debug.WriteLine($"\r\nRead: {total:N0} in {(DateTime.UtcNow - start).TotalMilliseconds:N0}ms");

                WaitForButton();
            }

            socket.Close();
        }

        private static void WaitForButton() {
            while (btn1.Read() == GpioPinValue.High) {
                led1.Write(led1.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

                Thread.Sleep(50);
            }

            while (btn1.Read() == GpioPinValue.Low)
                Thread.Sleep(50);
        }

        private static string WindToName(SPWF04SxIndication wind) {
            switch (wind) {
                case SPWF04SxIndication.ConsoleActive: return nameof(SPWF04SxIndication.ConsoleActive);
                case SPWF04SxIndication.PowerOn: return nameof(SPWF04SxIndication.PowerOn);
                case SPWF04SxIndication.Reset: return nameof(SPWF04SxIndication.Reset);
                case SPWF04SxIndication.WatchdogRunning: return nameof(SPWF04SxIndication.WatchdogRunning);
                case SPWF04SxIndication.LowMemory: return nameof(SPWF04SxIndication.LowMemory);
                case SPWF04SxIndication.WiFiHardwareFailure: return nameof(SPWF04SxIndication.WiFiHardwareFailure);
                case SPWF04SxIndication.ConfigurationFailure: return nameof(SPWF04SxIndication.ConfigurationFailure);
                case SPWF04SxIndication.HardFault: return nameof(SPWF04SxIndication.HardFault);
                case SPWF04SxIndication.StackOverflow: return nameof(SPWF04SxIndication.StackOverflow);
                case SPWF04SxIndication.MallocFailed: return nameof(SPWF04SxIndication.MallocFailed);
                case SPWF04SxIndication.RadioStartup: return nameof(SPWF04SxIndication.RadioStartup);
                case SPWF04SxIndication.WiFiPSMode: return nameof(SPWF04SxIndication.WiFiPSMode);
                case SPWF04SxIndication.Copyright: return nameof(SPWF04SxIndication.Copyright);
                case SPWF04SxIndication.WiFiBssRegained: return nameof(SPWF04SxIndication.WiFiBssRegained);
                case SPWF04SxIndication.WiFiSignalLow: return nameof(SPWF04SxIndication.WiFiSignalLow);
                case SPWF04SxIndication.WiFiSignalOk: return nameof(SPWF04SxIndication.WiFiSignalOk);
                case SPWF04SxIndication.BootMessages: return nameof(SPWF04SxIndication.BootMessages);
                case SPWF04SxIndication.KeytypeNotImplemented: return nameof(SPWF04SxIndication.KeytypeNotImplemented);
                case SPWF04SxIndication.WiFiJoin: return nameof(SPWF04SxIndication.WiFiJoin);
                case SPWF04SxIndication.WiFiJoinFailed: return nameof(SPWF04SxIndication.WiFiJoinFailed);
                case SPWF04SxIndication.WiFiScanning: return nameof(SPWF04SxIndication.WiFiScanning);
                case SPWF04SxIndication.ScanBlewUp: return nameof(SPWF04SxIndication.ScanBlewUp);
                case SPWF04SxIndication.ScanFailed: return nameof(SPWF04SxIndication.ScanFailed);
                case SPWF04SxIndication.WiFiUp: return nameof(SPWF04SxIndication.WiFiUp);
                case SPWF04SxIndication.WiFiAssociationSuccessful: return nameof(SPWF04SxIndication.WiFiAssociationSuccessful);
                case SPWF04SxIndication.StartedAP: return nameof(SPWF04SxIndication.StartedAP);
                case SPWF04SxIndication.APStartFailed: return nameof(SPWF04SxIndication.APStartFailed);
                case SPWF04SxIndication.StationAssociated: return nameof(SPWF04SxIndication.StationAssociated);
                case SPWF04SxIndication.DhcpReply: return nameof(SPWF04SxIndication.DhcpReply);
                case SPWF04SxIndication.WiFiBssLost: return nameof(SPWF04SxIndication.WiFiBssLost);
                case SPWF04SxIndication.WiFiException: return nameof(SPWF04SxIndication.WiFiException);
                case SPWF04SxIndication.WiFiHardwareStarted: return nameof(SPWF04SxIndication.WiFiHardwareStarted);
                case SPWF04SxIndication.WiFiNetwork: return nameof(SPWF04SxIndication.WiFiNetwork);
                case SPWF04SxIndication.WiFiUnhandledEvent: return nameof(SPWF04SxIndication.WiFiUnhandledEvent);
                case SPWF04SxIndication.WiFiScan: return nameof(SPWF04SxIndication.WiFiScan);
                case SPWF04SxIndication.WiFiUnhandledIndication: return nameof(SPWF04SxIndication.WiFiUnhandledIndication);
                case SPWF04SxIndication.WiFiPoweredDown: return nameof(SPWF04SxIndication.WiFiPoweredDown);
                case SPWF04SxIndication.HWInMiniAPMode: return nameof(SPWF04SxIndication.HWInMiniAPMode);
                case SPWF04SxIndication.WiFiDeauthentication: return nameof(SPWF04SxIndication.WiFiDeauthentication);
                case SPWF04SxIndication.WiFiDisassociation: return nameof(SPWF04SxIndication.WiFiDisassociation);
                case SPWF04SxIndication.WiFiUnhandledManagement: return nameof(SPWF04SxIndication.WiFiUnhandledManagement);
                case SPWF04SxIndication.WiFiUnhandledData: return nameof(SPWF04SxIndication.WiFiUnhandledData);
                case SPWF04SxIndication.WiFiUnknownFrame: return nameof(SPWF04SxIndication.WiFiUnknownFrame);
                case SPWF04SxIndication.Dot11Illegal: return nameof(SPWF04SxIndication.Dot11Illegal);
                case SPWF04SxIndication.WpaCrunchingPsk: return nameof(SPWF04SxIndication.WpaCrunchingPsk);
                case SPWF04SxIndication.WpaTerminated: return nameof(SPWF04SxIndication.WpaTerminated);
                case SPWF04SxIndication.WpaStartFailed: return nameof(SPWF04SxIndication.WpaStartFailed);
                case SPWF04SxIndication.WpaHandshakeComplete: return nameof(SPWF04SxIndication.WpaHandshakeComplete);
                case SPWF04SxIndication.GpioInterrupt: return nameof(SPWF04SxIndication.GpioInterrupt);
                case SPWF04SxIndication.Wakeup: return nameof(SPWF04SxIndication.Wakeup);
                case SPWF04SxIndication.PendingData: return nameof(SPWF04SxIndication.PendingData);
                case SPWF04SxIndication.InputToRemote: return nameof(SPWF04SxIndication.InputToRemote);
                case SPWF04SxIndication.OutputFromRemote: return nameof(SPWF04SxIndication.OutputFromRemote);
                case SPWF04SxIndication.SocketClosed: return nameof(SPWF04SxIndication.SocketClosed);
                case SPWF04SxIndication.IncomingSocketClient: return nameof(SPWF04SxIndication.IncomingSocketClient);
                case SPWF04SxIndication.SocketClientGone: return nameof(SPWF04SxIndication.SocketClientGone);
                case SPWF04SxIndication.SocketDroppingData: return nameof(SPWF04SxIndication.SocketDroppingData);
                case SPWF04SxIndication.RemoteConfiguration: return nameof(SPWF04SxIndication.RemoteConfiguration);
                case SPWF04SxIndication.FactoryReset: return nameof(SPWF04SxIndication.FactoryReset);
                case SPWF04SxIndication.LowPowerMode: return nameof(SPWF04SxIndication.LowPowerMode);
                case SPWF04SxIndication.GoingIntoStandby: return nameof(SPWF04SxIndication.GoingIntoStandby);
                case SPWF04SxIndication.ResumingFromStandby: return nameof(SPWF04SxIndication.ResumingFromStandby);
                case SPWF04SxIndication.GoingIntoDeepSleep: return nameof(SPWF04SxIndication.GoingIntoDeepSleep);
                case SPWF04SxIndication.ResumingFromDeepSleep: return nameof(SPWF04SxIndication.ResumingFromDeepSleep);
                case SPWF04SxIndication.StationDisassociated: return nameof(SPWF04SxIndication.StationDisassociated);
                case SPWF04SxIndication.SystemConfigurationUpdated: return nameof(SPWF04SxIndication.SystemConfigurationUpdated);
                case SPWF04SxIndication.RejectedFoundNetwork: return nameof(SPWF04SxIndication.RejectedFoundNetwork);
                case SPWF04SxIndication.RejectedAssociation: return nameof(SPWF04SxIndication.RejectedAssociation);
                case SPWF04SxIndication.WiFiAuthenticationTimedOut: return nameof(SPWF04SxIndication.WiFiAuthenticationTimedOut);
                case SPWF04SxIndication.WiFiAssociationTimedOut: return nameof(SPWF04SxIndication.WiFiAssociationTimedOut);
                case SPWF04SxIndication.MicFailure: return nameof(SPWF04SxIndication.MicFailure);
                case SPWF04SxIndication.UdpBroadcast: return nameof(SPWF04SxIndication.UdpBroadcast);
                case SPWF04SxIndication.WpsGeneratedDhKeyset: return nameof(SPWF04SxIndication.WpsGeneratedDhKeyset);
                case SPWF04SxIndication.WpsEnrollmentAttemptTimedOut: return nameof(SPWF04SxIndication.WpsEnrollmentAttemptTimedOut);
                case SPWF04SxIndication.SockdDroppingClient: return nameof(SPWF04SxIndication.SockdDroppingClient);
                case SPWF04SxIndication.NtpServerDelivery: return nameof(SPWF04SxIndication.NtpServerDelivery);
                case SPWF04SxIndication.DhcpFailedToGetLease: return nameof(SPWF04SxIndication.DhcpFailedToGetLease);
                case SPWF04SxIndication.MqttPublished: return nameof(SPWF04SxIndication.MqttPublished);
                case SPWF04SxIndication.MqttClosed: return nameof(SPWF04SxIndication.MqttClosed);
                case SPWF04SxIndication.WebSocketData: return nameof(SPWF04SxIndication.WebSocketData);
                case SPWF04SxIndication.WebSocketClosed: return nameof(SPWF04SxIndication.WebSocketClosed);
                case SPWF04SxIndication.FileReceived: return nameof(SPWF04SxIndication.FileReceived);
                default: return "Other";
            }
        }
    }
}
