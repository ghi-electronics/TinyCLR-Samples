using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Adc;

namespace Mikro.Click {
    class MicClick {
        private readonly AdcChannel analog;
        public MicClick(int adcController) {
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
