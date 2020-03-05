using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Adc;
using Mikro.Click;

namespace Mic_Click {
    class Program {
        private static int x;
        static void Main() {

            ////////// Set these to match your board //////////////
            var adcChannel = SC20100.AdcChannel.Controller1.PC0;
            var adcController = SC20100.AdcChannel.Controller1.Id;
            ///////////////////////////////////////////////////////

            var mic = new MicClick(AdcController.FromName(adcController).OpenChannel(adcChannel));

            //TestWithSimpleDebug(mic);
            TestWithGraph(mic);

            //never get here!
            Thread.Sleep(Timeout.Infinite);

        }
        static void TestWithSimpleDebug(MicClick mic) {
            while (true) {
                Debug.WriteLine(mic.Read().ToString());
                Thread.Sleep(100);
            }
        }
        static void TestWithGraph(MicClick mic) {

            const int SCREEN_WIDTH = 160;
            const int SCREEN_HEIGHT = 128;
            int graph;
            
            var spi = SpiController.FromName(SC20100.SpiBus.Spi3);
            var gpio = GpioController.GetDefault();

            //Display Setup
            var st7735 = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings
                (SpiChipSelectType.Gpio, SC20100.GpioPin.PD10)), //CS pin.
                gpio.OpenPin(SC20100.GpioPin.PC4), //RS pin.
                gpio.OpenPin(SC20100.GpioPin.PE15) //RESET pin.
            );

            var backlight = gpio.OpenPin(SC20100.GpioPin.PE5);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            var displayController = DisplayController.FromProvider(st7735);
            st7735.SetDataAccessControl(true, true, false, false); //Rotate the screen.

            displayController.SetConfiguration(new SpiDisplayControllerSettings {
                Width = SCREEN_WIDTH,
                Height = SCREEN_HEIGHT,
                DataFormat = DisplayDataFormat.Rgb565
            });

            displayController.Enable();

            // Create flush event
            Graphics.OnFlushEvent += Graphics_OnFlushEvent;

            var screen = Graphics.FromImage(new Bitmap
                (displayController.ActiveConfiguration.Width,
                displayController.ActiveConfiguration.Height));

            while (true) {

                graph = (int)(mic.Read() * 100) + 20;

                if (x++ > SCREEN_WIDTH) {
                    x = 0;
                    screen.Clear(Color.Black);
                }
                screen.DrawLine(new Pen(Color.Green), x, SCREEN_HEIGHT, x, graph);
                screen.Flush();
            }
            void Graphics_OnFlushEvent(IntPtr hdc, byte[] data) {
                st7735.DrawBuffer(data);
            }
        }
    }
}

