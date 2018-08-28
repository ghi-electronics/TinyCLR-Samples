using GHIElectronics.TinyCLR.Devices.Adc;

namespace SeeedGroveStarterKit {
    public class SoundSensor {
        private AdcChannel Channel;
        public SoundSensor(int AdcPinNumber) {
            Channel = AdcController.GetDefault().OpenChannel(AdcPinNumber);
        }
        // between 0 and 100
        public double ReadLevel() {
            double LastRead = 0;
            for (int i = 0; i < 10; i++) {
                double d = Channel.ReadRatio();
                if (d > LastRead)
                    LastRead = d;
            }
            return LastRead * 100;
        }
    }
}
