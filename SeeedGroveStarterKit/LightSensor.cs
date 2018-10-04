using GHIElectronics.TinyCLR.Devices.Adc;

namespace SeeedGroveStarterKit {
    public class LightSensor {
        private AdcChannel Channel;
        public LightSensor(int AdcPinNumber) => this.Channel = AdcController.GetDefault().OpenChannel(AdcPinNumber);
        
        // between 0 and 100
        public double ReadLightLevel() => this.Channel.ReadRatio() * 100;
    }
}
