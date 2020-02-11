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
            var led3 = gpio.OpenPin(G120E.GpioPin.P3_28);
            var led4 = gpio.OpenPin(G120E.GpioPin.P3_21);
            led3.SetDriveMode(GpioPinDriveMode.Output);
            led4.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led3.Write(GpioPinValue.High);
                led4.Write(GpioPinValue.Low);
                Thread.Sleep(150);
                led3.Write(GpioPinValue.Low);
                led4.Write(GpioPinValue.High);
                Thread.Sleep(400);
            }
        }
        static void Main() {
            var gpio = GpioController.GetDefault();
            var led1 = gpio.OpenPin(G120E.GpioPin.P3_27);
           led1.SetDriveMode(GpioPinDriveMode.Output);
            // This code will run in its own thread
            new Thread(Alarm).Start();
            // This is the main loop
            var state = false;
            while (true) {
                led1.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                state = !state;
                Thread.Sleep(100);
            }
        }
    }
}
