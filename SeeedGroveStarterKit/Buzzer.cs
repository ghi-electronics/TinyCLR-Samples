using GHIElectronics.TinyCLR.Devices.Gpio;
using System.Threading;

namespace SeeedGroveStarterKit {
    public class Buzzer {
        private GpioPin Pin;
        public Buzzer(int GpioPinNumber) {
            this.Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            this.Pin.Write(GpioPinValue.Low);
            this.Pin.SetDriveMode(GpioPinDriveMode.Output);
        }
        public void TurnOn() {
            this.Pin.Write(GpioPinValue.High);
        }
        public void TurnOff() {
            this.Pin.Write(GpioPinValue.Low);
        }
        public void Beep() {
            this.TurnOn();
            Thread.Sleep(10);
            this.TurnOff();
        }
    }
}
