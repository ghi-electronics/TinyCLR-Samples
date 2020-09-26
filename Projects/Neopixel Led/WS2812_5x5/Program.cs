using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Drivers.Neopixel.WS2812;
using GHIElectronics.TinyCLR.Pins;

namespace WS2812_Led {
    class Program {
        const int NUM_LED = 25;

        static void Main() {
            var signalPin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA0);
            var ledController = new WS2812Controller(signalPin, NUM_LED);

            ledController.SetColor(0, 0xFF, 0xFF, 0xFF);
            ledController.SetColor(1, 0x00, 0xFF, 0xFF);
            ledController.SetColor(2, 0x00, 0x00, 0xFF);

            ledController.SetColor(24, 0xFF, 0xFF, 0xFF);
            ledController.SetColor(23, 0x00, 0xFF, 0xFF);
            ledController.SetColor(22, 0xFF, 0x00, 0x00);

            while (true)
                ledController.Flush();
        }
    }
}
