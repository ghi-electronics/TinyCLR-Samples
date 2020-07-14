
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;


namespace Demos {
    class Program {
        static void Main() {
            var gpioController = GpioController.GetDefault();

            var led = gpioController.OpenPin(SC20100.GpioPin.PE11);
            led.SetDriveMode(GpioPinDriveMode.Output);

            while (true) {
                led.Write(led.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                Thread.Sleep(100);
            }

        }
    }
}
