using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.I2c;
using Mikro.Click;

namespace FM_Click {
    class Program {       
        private static void Main() {

        ////////// Set these to match your board //////////////
        var clickRstPin = SC20100.GpioPin.PD4;
        var clickCsPin = SC20100.GpioPin.PD3;
        var clicki2cbus = SC20100.I2cBus.I2c1;
        ///////////////////////////////////////////////////////  

        var radio = new FMClick(clickRstPin, clickCsPin, I2cController.FromName(clicki2cbus));
            double currentStation = 101.9;
            int volume = 255;
            radio.Channel = currentStation;
            radio.Volume = volume;
        }
    }
}
