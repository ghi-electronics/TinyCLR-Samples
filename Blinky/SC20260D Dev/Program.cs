using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Blinky{
    class Program{
        static void Main(){
            // Blink SC20260D LED PH6
            var led = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PH6);
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