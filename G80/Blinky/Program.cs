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
            var LED = GpioController.GetDefault().OpenPin(G80.GpioPin.PE14);
            LED.SetDriveMode(GpioPinDriveMode.Output);
            var state = false;
            while (true) {
                LED.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                state = !state;
                Thread.Sleep(100);
            }
        }
    }
}
