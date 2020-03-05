using GHIElectronics.TinyCLR.Devices.Adc;

namespace Mikro.Click {
    
    class AlcoholClick {
        private readonly AdcChannel analog;
        public AlcoholClick(AdcChannel adcChannel) => this.analog = adcChannel;
        public double Read() {
           var ratio = this.analog.ReadRatio();
           return ratio;
        }
    }
}
