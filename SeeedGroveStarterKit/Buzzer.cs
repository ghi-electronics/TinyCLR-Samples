using GHIElectronics.TinyCLR.Devices.Gpio;
using System.Threading;

namespace SeeedGroveStarterKit {
    public class Buzzer {
        private GpioPin Pin;
        public Buzzer(int GpioPinNumber) {
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
        public void Beep() {
            TurnOn();
            Thread.Sleep(10);
            TurnOff();
        }
    }
}
