using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System.Diagnostics;

namespace TouchClamp_Click {
    class Program {
        public static string i2cbus = SC20100.I2cBus.I2c1;

        static void Main() {

            TouchClamp piano = new TouchClamp(i2cbus);

            while (true) {
                Debug.WriteLine(piano.GetKey().ToString());
            }      
        }
    }
}
