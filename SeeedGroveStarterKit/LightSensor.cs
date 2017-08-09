using GHIElectronics.TinyCLR.Devices.Adc;

namespace SeeedGroveStarterKit {
    public class LightSensor {
        private AdcChannel Channel;
        public LightSensor(int AdcPinNumber) {
            Channel = AdcController.GetDefault().OpenChannel(AdcPinNumber);
        }
        // between 0 and 100
        public double ReadLightLevel() {
            return Channel.ReadRatio() * 100;
        }
    }
}
