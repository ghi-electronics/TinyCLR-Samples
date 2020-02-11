using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Blinky{
    class Program{
        static void Main(){
            // Blink SC20100 LED PB0
            var led = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PB0);
            led.SetDriveMode(GpioPinDriveMode.Output);
            var state = false;

            while (true){
                led.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                state = !state;
                Thread.Sleep(100);
            }
        }
    }
}