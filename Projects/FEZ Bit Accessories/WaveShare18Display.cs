//https://github.com/waveshare/WSLCD1in8/
// reset: P8
// RS: P12
// CS: P16
// BL: P1
// SPI: usual bus!

using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using System.Drawing;


namespace GHIElectronics.TinyCLR.Waveshare.Display18 {
    class WaveShare18Display {
        GpioPin backlight;
        ST7735Controller st7735;
        Graphics screen;
        public WaveShare18Display(GpioPin reset, GpioPin rs, GpioPin cs, SpiController spi, GpioPin backlight) {
            // Display Get Ready ////////////////////////////////////
            var gpio = GpioController.GetDefault();
            this.backlight = backlight;
            this.st7735 = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings

                (SpiChipSelectType.Gpio, cs)), //CS pin.
                    rs, //RS pin.
                    reset //RESET pin.
                );

            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            this.st7735.SetDataAccessControl(true, true, false, false); //Rotate the screen.
            this.st7735.SetDrawWindow(0, 0, 160, 128);
            this.st7735.Enable();
            // Create flush event
            Graphics.OnFlushEvent += this.Graphics_OnFlushEvent;
        }
        public void EnableBacklight(bool enable) {
            if (enable)
                this.backlight.Write(GpioPinValue.High);
            else
                this.backlight.Write(GpioPinValue.Low);
        }
        private void Graphics_OnFlushEvent(Graphics sender, byte[] data, int x, int y, int width, int height, int originalWidth) => this.st7735.DrawBuffer(data);
      
    }
}
