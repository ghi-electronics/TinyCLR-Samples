using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;

namespace Mikro.Click {
    class LedRingClick {

        private SpiDevice spi;
        private byte[] tempLedStateA = new byte[4];
        private GpioPin rst;
        public uint ledState;
        public LedRingClick(SpiController controller, int cs, int rst) {
            var settings = new SpiConnectionSettings() {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = cs,
                Mode = SpiMode.Mode0,
                ClockFrequency = 4_000_000,
                DataBitLength = 8,
            };
            this.spi = controller.GetDevice(settings);
            var gpio = GpioController.GetDefault();
            this.rst = gpio.OpenPin(rst);
            this.rst.SetDriveMode(GpioPinDriveMode.Output);
            this.rst.Write(GpioPinValue.High);
            this.Clear();
        }
        public void Update() {
            this.tempLedStateA[0] = (byte)(this.ledState >> 0);
            this.tempLedStateA[1] = (byte)(this.ledState >> 8);
            this.tempLedStateA[2] = (byte)(this.ledState >> 16);
            this.tempLedStateA[3] = (byte)(this.ledState >> 24);
            this.spi.Write(this.tempLedStateA);
        }
        public void Clear() {
            this.ledState = 0;
            this.Update();
        }
    }
}
