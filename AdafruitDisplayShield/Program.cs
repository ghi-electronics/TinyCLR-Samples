using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;

using Adafruit.Display18;

namespace AdafruitDisplayShield {
    class Program {
        static void Main() {
            Display DisplayShield = new Display(FEZ.GpioPin.D8, FEZ.GpioPin.D10, FEZ.SpiBus.Spi1);
            int i = 0;
            DisplayShield.DrawLargeText(20, 30, "TinyCLR OS", Color.Green);
            while (true) {
                DisplayShield.DrawText(30, 60, "Count: " + i++, Color.Magenta);
                Thread.Sleep(10);
            }
        }
    }
}
