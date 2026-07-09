using GHIElectronics.TinyCLR.UI.Controls;

namespace Demos {
    // Code-behind for WifiContent.tcui. Exposes the designed elements the hosting WifiWindow
    // (an ApplicationWindow) drives at runtime — the five network-property labels + status line it fills
    // from the WINC15x0 events, the SSID/password text boxes it reads, and the Connect button it wires.
    public partial class WifiContent : Canvas {
        public WifiContent() => InitializeComponent();

        public Text IpAddressLabel => this._ipAddressLabel;
        public Text GatewayLabel => this._gatewayLabel;
        public Text SubnetMaskLabel => this._subnetMaskLabel;
        public Text DnsLabel1 => this._dnsLabel1;
        public Text DnsLabel2 => this._dnsLabel2;
        public Text Status => this._status;

        public TextBox Ssid => this._ssid;
        public TextBox Password => this._password;

        public Button ConnectButton => this._connectButton;
    }
}
