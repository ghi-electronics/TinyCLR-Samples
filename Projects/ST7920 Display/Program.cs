using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Drivers.ST7920;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;

namespace ST7920_Display {
    class Program {
        static void Main() {
            var reset = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PC5);
            var chipsetlect = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PC4);
            var spichannel = SpiController.FromName(SC20100.SpiBus.Spi6);
            var display = new ST7920Controller(reset, chipsetlect, spichannel);

            var r = new Random();
            while (true) {
                display.Pixel(r.Next(100), r.Next(60), true);
                display.Pixel(r.Next(100), r.Next(60), false);
                display.Display_print();
                Thread.Sleep(50);
            }

        }
    }
}
