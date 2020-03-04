using System.Diagnostics;
using GHIElectronics.TinyCLR.Pins;
using Mikro.Click;

namespace Alcohol_Click {
    class Program {
        static void Main() {

            ////////// Set these to match your board //////////////
            var adc = SC20100.AdcChannel.Controller1.PC0;
            ///////////////////////////////////////////////////////

            var sensor = new AlcoholClick(adc);

            while (true) {

                Debug.WriteLine(sensor.Read().ToString());
            }
        }
    }
}
