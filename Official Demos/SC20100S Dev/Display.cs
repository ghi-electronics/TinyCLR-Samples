using System.Drawing;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;

namespace Demos {
    static class Display {
        public const int Width = 160;
        public const int Height = 128;

        public static ST7735Controller DisplayController;

        public static void InitializeDisplay() {
            var spi = SpiController.FromName(SC20100.SpiBus.Spi4);
            var gpio = GpioController.GetDefault();

            DisplayController = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, gpio.OpenPin(SC20100.GpioPin.PD10))),
                gpio.OpenPin(SC20100.GpioPin.PC4),  // RS
                gpio.OpenPin(SC20100.GpioPin.PE15)  // RESET
            );

            var backlight = gpio.OpenPin(SC20100.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            DisplayController.SetDataAccessControl(true, true, false, false); // rotate
            DisplayController.SetDrawWindow(0, 0, Width, Height);
            DisplayController.Enable();

            Graphics.OnFlushEvent += Graphics_OnFlushEvent;
        }

        private static void Graphics_OnFlushEvent(Graphics sender, byte[] data, int x, int y, int width, int height, int originalWidth) {
            while (true) {
                try {
                    DisplayController.DrawBuffer(data);
                    return;
                }
                catch {
                    // Retry on transient SPI errors.
                }
            }
        }
    }
}
