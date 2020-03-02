using System;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Pins;
using Mikro.Click;

namespace LedRing_Click {
    class Program {

        static void Main() {

            ////////// Set these to match your board //////////////
            var ClickSpiChannel = SC20100.SpiBus.Spi3;
            var ClickCsPin = SC20100.GpioPin.PD14;
            var ClickRsPin = SC20100.GpioPin.PD15;
            ///////////////////////////////////////////////////////

            var ring = new Mikro.Click.LedRingClick(ClickSpiChannel,ClickCsPin,ClickRsPin); 
            
            while (true) {
                uint fill = 1;
                for (var x = 0; x < 32; x++) {
                    ring.ledState += fill;
                    fill <<= 1;
                    ring.Update();
                    Thread.Sleep(30);
                }

                uint del = 1;
                for (var x = 0; x < 32; x++) {
                    ring.ledState -= del;
                    del <<= 1;
                    ring.Update();
                    Thread.Sleep(30);
                }
            }
        }
    }
}
