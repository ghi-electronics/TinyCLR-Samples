using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;
using Oled_Click.Properties;
using SSD1351;

namespace Oled_Click {
    class Program {

        static SSD1351Controller ssd1351;
        static void Main() {

            var gpioController = GpioController.GetDefault();

            // Turn backlight On
            var backlight = gpioController.OpenPin(SC20100.GpioPin.PA8);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            // SSD1351 Controller
            ssd1351 = new SSD1351Controller(SpiController.FromName(SC20100.SpiBus.Spi3).GetDevice(SSD1351Controller.GetConnectionSettings(SpiChipSelectType.Gpio, SC20100.GpioPin.PD14)), gpioController.OpenPin(SC20100.GpioPin.PA6), gpioController.OpenPin(SC20100.GpioPin.PD15));

            // TinyCLR Graphic
            var graphic = Graphics.FromImage(new Bitmap(ssd1351.MaxWidth, ssd1351.MaxHeight));
            var font = Resources.GetFont(Resources.FontResources.small);

            Graphics.OnFlushEvent += Graphics_OnFlushEvent;

            graphic.Clear(Color.Black);
            graphic.DrawString("TinyCLR OS 2.0", font, new SolidBrush(Color.Red), 30, 10);
            graphic.Flush();

        }

        private static void Graphics_OnFlushEvent(IntPtr hdc, byte[] data) => ssd1351.DrawBuffer(data);
    }
}
