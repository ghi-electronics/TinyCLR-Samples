using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;
using System.Drawing;

namespace FEZ_Bit {
    class Program {
        private static ST7735Controller st7735;
        private const int SCREEN_WIDTH = 160;
        private const int SCREEN_HEIGHT = 128;

        static void Blinker() {
            var led = GpioController.GetDefault().OpenPin(
                FEZBit.GpioPin.Led);
            led.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led.Write(GpioPinValue.High);
                Thread.Sleep(100);
                led.Write(GpioPinValue.Low);
                Thread.Sleep(900);
            }
        }
        static void Main() {
            new Thread(Blinker).Start();

            // Display Get Ready ////////////////////////////////////
            var spi = SpiController.FromName(FEZBit.SpiBus.Display);
            var gpio = GpioController.GetDefault();

            st7735 = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings
                (SpiChipSelectType.Gpio, FEZBit.GpioPin.DisplayCs)), //CS pin.
                gpio.OpenPin(FEZBit.GpioPin.DisplayRs), //RS pin.
                gpio.OpenPin(FEZBit.GpioPin.DisplayReset) //RESET pin.
            );

            var backlight = gpio.OpenPin(FEZBit.GpioPin.Backlight);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            st7735.SetDataAccessControl(true, true, false, false); //Rotate the screen.
            st7735.SetDrawWindow(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
            st7735.Enable();
            // Create flush event
            Graphics.OnFlushEvent += Graphics_OnFlushEvent; ;

            // Create bitmap buffer
            var screen = Graphics.FromImage(new Bitmap(SCREEN_WIDTH, SCREEN_HEIGHT));

            var font = Resource.GetFont(Resource.FontResources.droid_reg24);

            // Buzzer ///////////////////
            var pwmController3 = PwmController.FromName(FEZBit.PwmChannel.BuzzerController);
            var buzzer = pwmController3.OpenChannel(FEZBit.PwmChannel.BuzzerChannel);
            pwmController3.SetDesiredFrequency(500);
            buzzer.SetActiveDutyCyclePercentage(0.5);
            buzzer.Start();
            for (var f = 500; f < 5_000; f += 300) {
                pwmController3.SetDesiredFrequency(f);
                //Thread.Sleep(1);
            }
            buzzer.Stop();

            
            var x = 0;
            while (true) {
                screen.Clear();
                screen.DrawString(x++.ToString(), font, new SolidBrush(Color.Green), 10, 10);

                screen.Flush();
                Thread.Sleep(100);
            }
        }

        private static void Graphics_OnFlushEvent(IntPtr hdc, byte[] data) => st7735.DrawBuffer(data);
    }
}
