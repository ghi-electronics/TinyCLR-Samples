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
            var led1 = gpio.OpenPin(FEZ.GpioPin.Led1);
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

            // Blink the second LED here, once a second
            var gpio = GpioController.GetDefault();
            var led1 = gpio.OpenPin(FEZ.GpioPin.Led2);
            led1.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led1.Write(GpioPinValue.High);
                Thread.Sleep(200);
                led1.Write(GpioPinValue.Low);
                Thread.Sleep(800);
            }
        }
    }
}
