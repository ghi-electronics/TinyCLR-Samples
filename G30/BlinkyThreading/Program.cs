using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Blinky {
    class Program {
        static void Alarm() {
            var gpio = GpioController.GetDefault();
            var led1 = gpio.OpenPin(G30.GpioPin.PC7);
            led1.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led1.Write(GpioPinValue.High);
                Thread.Sleep(150);
                led1.Write(GpioPinValue.Low);
                Thread.Sleep(400);
            }
        }
        static void Main() {
            // This code will run in its own thread
            new Thread(Alarm).Start();

            // Sleep forever!
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
