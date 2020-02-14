using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System.Diagnostics;

namespace Blinky {
    class Program {
        static void Main() {
            var GPIO = GpioController.GetDefault();
            GpioPin led;
            Debug.WriteLine(GHIElectronics.TinyCLR.Native.DeviceInformation.DeviceName);

            if (GHIElectronics.TinyCLR.Native.DeviceInformation.DeviceName == "SC20100")
                led = GPIO.OpenPin(SC20100.GpioPin.PB0);
            else
                led = GPIO.OpenPin(SC20260.GpioPin.PH6);

            led.SetDriveMode(GpioPinDriveMode.Output);
            var state = false;

            while (true) {
                led.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                state = !state;
                Thread.Sleep(100);
            }
        }
    }
}