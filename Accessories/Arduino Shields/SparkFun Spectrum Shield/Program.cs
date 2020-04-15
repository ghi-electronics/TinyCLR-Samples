using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.MixedSignalIntegrated.MSGEQ7;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace SpectrumShield
{
    class Program
    {
        static ST7735Controller st7735;
        static Graphics graphic;

        const int SCREEN_WIDTH = 160;
        const int SCREEN_HEIGHT = 128;

        static void Main()
        {
            var adcController = AdcController.FromName(SC20100.AdcChannel.Controller1.Id);

            var adcChannel = adcController.OpenChannel(SC20100.AdcChannel.Controller1.PA0);

            var strobePin = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PE1);
            var resetPin = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PE0);

            var msgeq7 = new Msgeq7(adcChannel, strobePin, resetPin);

            InitializeSPIDisplay();

            while (true)
            {
                msgeq7.UpdateBands();

                DrawEqualizer(msgeq7.Data);

                GC.Collect();

                GC.WaitForPendingFinalizers();
            }

        }

        static void InitializeSPIDisplay()
        {
            var spi = SpiController.FromName(SC20100.SpiBus.Spi3);

            var gpio = GpioController.GetDefault();

            var csPin = gpio.OpenPin(SC20100.GpioPin.PD10);

            st7735 = new ST7735Controller(

                spi.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, csPin)), // CS PD10

                gpio.OpenPin(SC20100.GpioPin.PC4), // PE10 RS

                gpio.OpenPin(SC20100.GpioPin.PE15) // PE10 RESET

            );


            st7735.SetDataAccessControl(true, true, true, false);

            var bl = gpio.OpenPin(SC20100.GpioPin.PE5); // Backligth PC7

            bl.SetDriveMode(GpioPinDriveMode.Output);

            bl.Write(GpioPinValue.High);

            Graphics.OnFlushEvent += Graphics_OnFlushEvent;

            graphic = Graphics.FromImage(new Bitmap(160, 128));
        }
        static void DrawEqualizer(int[] data)
        {
            var channel1_x = 0;
            var channel_xOffset = SCREEN_WIDTH / 16;
            var channel_y = 0;
            var channel_w = SCREEN_WIDTH / 20;

            var channel2_x = (SCREEN_WIDTH >>1) + channel_xOffset;

            graphic.Clear();

            for (var i = 0; i < 7; i++)
            {
                var valueScaledLeft = (data[i] * (SCREEN_HEIGHT * 3 /4)) / 65535;

                graphic.FillRectangle(new SolidBrush(Color.Blue), channel1_x + channel_xOffset * i, channel_y, channel_w, valueScaledLeft);
                
            }


            graphic.Flush();


            Thread.Sleep(1);
        }
        private static void Graphics_OnFlushEvent(IntPtr hdc, byte[] data) => st7735.DrawBuffer(data);
    }
}
