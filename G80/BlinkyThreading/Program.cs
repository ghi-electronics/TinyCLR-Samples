using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Blinky {
    class Program {
        static void Alarm() {
            var GPIO = GpioController.GetDefault();
            var LED3 = GPIO.OpenPin(G80.GpioPin.PE11);
            var LED4 = GPIO.OpenPin(G80.GpioPin.PE9);
            LED3.SetDriveMode(GpioPinDriveMode.Output);
            LED4.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                LED3.Write(GpioPinValue.High);
                LED4.Write(GpioPinValue.Low);
                Thread.Sleep(150);
                LED3.Write(GpioPinValue.Low);
                LED4.Write(GpioPinValue.High);
                Thread.Sleep(400);
            }
        }
        static void Main() {
            var GPIO = GpioController.GetDefault();
            var LED1 = GPIO.OpenPin(G80.GpioPin.PE14);
           LED1.SetDriveMode(GpioPinDriveMode.Output);
            // This code will run in its own thread
            new Thread(Alarm).Start();
            // This is the main loop
            var state = false;
            while (true) {
                LED1.Write(state ? GpioPinValue.High : GpioPinValue.Low);
                state = !state;
                Thread.Sleep(100);
            }
        }
    }
}
