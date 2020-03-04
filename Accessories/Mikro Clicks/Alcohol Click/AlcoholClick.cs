using GHIElectronics.TinyCLR.Devices.Adc;

namespace Mikro.Click {
    
    class AlcoholClick {

        private readonly AdcChannel analog;   
        public AlcoholClick(int adcController) {
            var adc = AdcController.GetDefault();
            var analogChannel = adc.OpenChannel(adcController);
            this.analog = analogChannel;         
        }
        public double Read() {
           var ratio = this.analog.ReadRatio();
           return ratio;
        }
    }
}
