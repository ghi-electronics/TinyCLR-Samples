using System;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace FEZ_Feather {
    class Program {
        static void Blinker() {
            var led = GpioController.GetDefault().OpenPin(
                SC20100.GpioPin.PE11);
            led.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led.Write(GpioPinValue.High);
                Thread.Sleep(100);
                led.Write(GpioPinValue.Low);
                Thread.Sleep(100);
            }
        }
        static void Main() {
            new Thread(Blinker).Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
