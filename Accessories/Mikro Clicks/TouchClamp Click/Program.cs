using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Pins;
using System.Diagnostics;
using Mikro.Click;

namespace TouchClamp_Click {
    class Program {
        static void Main() {
            ////////// Set these to match your board ////////
            var clicki2cbus = SC20100.I2cBus.I2c1;
            ////////////////////////////////////////////////

            var pad = new TouchClamp(I2cController.FromName(clicki2cbus));

            while (true) {

                Debug.WriteLine(pad.GetKey().ToString());
            }
        }
    }
}
