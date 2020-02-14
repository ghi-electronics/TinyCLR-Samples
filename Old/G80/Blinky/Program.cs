using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Blinky {
    class Program {
        static void Main() {
            // Blink LED1
            var led = GpioController.GetDefault().OpenPin(G80.GpioPin.PE14);
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
