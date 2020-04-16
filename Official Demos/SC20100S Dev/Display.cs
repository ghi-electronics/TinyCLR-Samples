using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;

namespace Demos {
    static class Display {
        static ST7735Controller st7735 = null;

        public static void InitializeDisplay() {
            var spi = SpiController.FromName(SC20100.SpiBus.Spi4);
            var gpio = GpioController.GetDefault();

            st7735 = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, gpio.OpenPin(SC20100.GpioPin.PD10))), // ChipSelect 
                gpio.OpenPin(SC20100.GpioPin.PC4), // Pin RS
                gpio.OpenPin(SC20100.GpioPin.PE15) // Pin RESET

            );

            var bl = gpio.OpenPin(SC20100.GpioPin.PA15); // back light

            bl.Write(GpioPinValue.High);
            bl.SetDriveMode(GpioPinDriveMode.Output);

            st7735.SetDataAccessControl(true, true, false, false); //Rotate the screen.
            st7735.SetDrawWindow(0, 0, Width, Height);
            st7735.Enable();


            // Create flush event 
            Graphics.OnFlushEvent += Graphics_OnFlushEvent;
        }

        private static void Graphics_OnFlushEvent(IntPtr hdc, byte[] data) => st7735.DrawBuffer(data);

        public static int Width => 160;
        public static int Height => 128;
    }
}
