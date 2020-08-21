using GHIElectronics.TinyCLR.Drivers.Neopixel.WS2812;
using GHIElectronics.TinyCLR.Pins;

namespace WS2812_Led {
    class Program {
        const int NUM_LED = 25;

        static void Main() {
            var leds = new WS2812(SC20260.GpioPin.PA0, NUM_LED);

            leds.SetColor(0, 0xFF, 0xFF, 0xFF);
            leds.SetColor(1, 0x00, 0xFF, 0xFF);
            leds.SetColor(2, 0x00, 0x00, 0xFF);

            leds.SetColor(24, 0xFF, 0xFF, 0xFF);
            leds.SetColor(23, 0x00, 0xFF, 0xFF);
            leds.SetColor(22, 0xFF, 0x00, 0x00);

            while (true)
                leds.Draw();
        }
    }
}
