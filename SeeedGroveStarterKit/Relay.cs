using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGroveStarterKit {
    public class Relay {
        GpioPin Pin;
        public Relay(int GpioPinNumber) {
            Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            Pin.Write(GpioPinValue.Low);
            Pin.SetDriveMode(GpioPinDriveMode.Output);
        }
        public void TurnOn() {
            Pin.Write(GpioPinValue.High);
        }
        public void TurnOff() {
            Pin.Write(GpioPinValue.Low);
        }
    }
}
