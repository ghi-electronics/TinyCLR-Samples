using System.Diagnostics;
using GHIElectronics.TinyCLR.Pins;
using Mikro.Click;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace Mic_Click {
    class Program {
        static void Main() {

            ////////// Set these to match your board //////////////
            var adc = SC20100.AdcChannel.Controller1.PC0;
            ///////////////////////////////////////////////////////

            var sensor = new MicClick(adc);

            while (true) {
                Debug.WriteLine(sensor.Read().ToString());
                Thread.Sleep(100);

            }
        }
    }
}
