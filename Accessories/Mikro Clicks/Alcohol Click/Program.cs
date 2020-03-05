using System.Diagnostics;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Adc;
using Mikro.Click;

namespace Alcohol_Click {
    class Program {
        static void Main() {

            ////////// Set these to match your board //////////////
            var adcChannel = SC20100.AdcChannel.Controller1.PC0;
            var adcController = SC20100.AdcChannel.Controller1.Id;
            ///////////////////////////////////////////////////////

            var sensor = new AlcoholClick(AdcController.FromName(adcController).OpenChannel(adcChannel));

            while (true) {

                Debug.WriteLine(sensor.Read().ToString());
            }
        }
    }
}
