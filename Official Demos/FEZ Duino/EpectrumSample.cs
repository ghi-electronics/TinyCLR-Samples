// https://cdn.sparkfun.com/datasheets/Dev/Arduino/Shields/Spectrum_Shield_v16.pdf
// strobe D4
// Reset D5
// Analog left channel A0
// No need right channel

using System;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Adafruit.LPD8806;
using GHIElectronics.TinyCLR.Drivers.MixedSignalIntegrated.MSGEQ7;
using GHIElectronics.TinyCLR.Pins;

namespace FEZ_Duino {
    static class EpectrumSample {
        static LPD8806 mLedStrip;

        const int LEDS_PER_COLUMN = 8;
        const int TOTAL_COLUMN = 7;
        const int TOTAL_LEDS = LEDS_PER_COLUMN * TOTAL_COLUMN;
        static public void Run() {

            var ain = AdcController.FromName(SC20100.AdcChannel.Controller1.Id).
                OpenChannel(SC20100.AdcChannel.Controller1.PA4);
            var strobe = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PA2);
            var reset = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PC7);
            var msgeq7 = new Msgeq7(ain, strobe, reset);
            var spiController = SpiController.FromName(SC20100.SpiBus.Spi3);

            mLedStrip = new LPD8806(spiController, TOTAL_LEDS);

            while (true) {
                msgeq7.UpdateBands();

                DrawEqualizer(msgeq7.Data);

                Thread.Sleep(10);

            }
        }
        static void DrawEqualizer(int[] data) {

            for (var i = 0; i < TOTAL_COLUMN; i++) {

                var valueScaledLeft = data[i] * LEDS_PER_COLUMN / 65535;

                for (var x = 0; x < LEDS_PER_COLUMN; x++) {
                    if (x <= valueScaledLeft) {
                        mLedStrip.SetColor((LEDS_PER_COLUMN * i) + x, 0, 0, 50);
                    }
                    else
                        mLedStrip.SetColor((LEDS_PER_COLUMN * i) + x, 0, 0, 0);
                }

            }

        }

    }

}
