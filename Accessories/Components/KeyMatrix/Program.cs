using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace KeyMatrix {
    class Program {

        static void Main() {

            ////////// Change these to your specific pin ///////////////
            var gpioController = GpioController.GetDefault();
            var pins = new GpioPin[8] {

                gpioController.OpenPin(SC20100.GpioPin.PE8),
                gpioController.OpenPin(SC20100.GpioPin.PA5),
                gpioController.OpenPin(SC20100.GpioPin.PA3),
                gpioController.OpenPin(SC20100.GpioPin.PB10),
                gpioController.OpenPin(SC20100.GpioPin.PC3),
                gpioController.OpenPin(SC20100.GpioPin.PC2),
                gpioController.OpenPin(SC20100.GpioPin.PE6),
                gpioController.OpenPin(SC20100.GpioPin.PE4),
            };
            ///////////////////////////////////////////////////////////

            var key = new KeypadMatrix(pins);
            key.KeyPressed += Key_KeyPressed;

            // Do something else here!
            Thread.Sleep(Timeout.Infinite);
        }

        private static void Key_KeyPressed(char c) => Debug.WriteLine(c.ToString());//throw new NotImplementedException();
    }
}
