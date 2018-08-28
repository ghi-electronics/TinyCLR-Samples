using GHIElectronics.TinyCLR.Devices.Adc;

namespace SeeedGroveStarterKit {
    public class TemperatureSensor {
        private AdcChannel Channel;
        public TemperatureSensor(int AdcPinNumber) {
            Channel = AdcController.GetDefault().OpenChannel(AdcPinNumber);
        }
        public double ReadTemperature() {
            // Per code example from seeed http://wiki.seeed.cc/Grove-Temperature_Sensor_V1.2/
            // Seemes to work fine with 3.3V
            double d = Channel.ReadRatio() * 1023;
            double r = 1023.0 / d - 1.0;
            double temperature = 1.0 / (System.Math.Log(r) / 4275 + 1 / 298.15) - 273.15;

            return temperature;
        }

    }
}
