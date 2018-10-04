using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGroveStarterKit {
    public class Relay {
        GpioPin Pin;
        public Relay(int GpioPinNumber) {
            this.Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            this.Pin.Write(GpioPinValue.Low);
            this.Pin.SetDriveMode(GpioPinDriveMode.Output);
        }
        public void TurnOn() => this.Pin.Write(GpioPinValue.High);
        
        public void TurnOff() => this.Pin.Write(GpioPinValue.Low);
    }
}
