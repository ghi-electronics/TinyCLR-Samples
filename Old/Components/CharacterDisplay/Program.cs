using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

using GHIElectronics.TinyCLR.Display.HD44780;

namespace CharacterDisplay {
    class Program {
        static void Main() {
            var gpio = GpioController.GetDefault();
            var D4 = new GpioPin[4]{
                gpio.OpenPin(FEZ.GpioPin.D4),
                gpio.OpenPin(FEZ.GpioPin.D5),
                gpio.OpenPin(FEZ.GpioPin.D6),
                gpio.OpenPin(FEZ.GpioPin.D7) };
            var E = gpio.OpenPin(FEZ.GpioPin.D9);
            var RS = gpio.OpenPin(FEZ.GpioPin.D8);
            var Display = new DisplayHD44780(D4, E, RS);

            Display.Clear();
            var counter = 0;
            while (true) {
                Display.CursorHome();
                Display.Print("Count: " + counter++);
                Thread.Sleep(20);

            }
        }
    }
}
