using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.ShijiLighting.APA102C;
using GHIElectronics.TinyCLR.Pins;

namespace APA102CLedStrip {
    public static class Program {
        public static void Main() {
            var spi1Controller = SpiController.FromName(FEZ.SpiBus.Spi1);

            var ledCount = 10;
            var apa102c = new APA102CController(spi1Controller.GetDevice(APA102CController.GetConnectionSettings()), ledCount);

            while (true) {
                for (var i = 0; i < ledCount; i++) {
                    apa102c.Set(i, Color.Blue);
                    apa102c.Flush();
                    Thread.Sleep(125);
                
                    apa102c.Set(i, Color.Black);
                    apa102c.Flush();
                    Thread.Sleep(125);
                }
                
                Thread.Sleep(2500);
            }
        }
    }
}
