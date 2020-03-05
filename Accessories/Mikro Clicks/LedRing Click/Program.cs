using System.Threading;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Spi;
using Mikro.Click;

namespace LedRing_Click {
    class Program {
        static void Main() {

            ////////// Set these to match your board //////////////
            var clickRstPin = SC20100.GpioPin.PD4;
            var clickCsPin = SC20100.GpioPin.PD3;
            var spiBus = SC20100.SpiBus.Spi3;
            ///////////////////////////////////////////////////////

            var controller = SpiController.FromName(spiBus);
            var ring = new LedRingClick(controller,clickCsPin,clickRstPin);
            
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
