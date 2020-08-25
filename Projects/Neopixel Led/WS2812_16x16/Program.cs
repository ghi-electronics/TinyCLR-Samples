using System;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Drivers.Neopixel.WS2812;
using GHIElectronics.TinyCLR.Pins;
using WS2812_16x16.Properties;

namespace WS2812_16x16 {
    class Program {
        const int NUM_LED = 256;
        static WS2812Controller leds;
        const string Text = "TinyCLR OS";

        static void Main() {
            var graphic = Graphics.FromImage(new Bitmap(16, 16));

            var font = Resources.GetFont(Resources.FontResources.small);

            Graphics.OnFlushEvent += Graphics_OnFlushEvent;

            var pin = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA0);

            leds = new WS2812Controller(pin, NUM_LED);

            var x = 9;

            var mode = 0;

            while (true) {

                graphic.Clear();

                var t1 = DateTime.Now.Ticks ;

                switch (mode) {
                    case 0:
                        graphic.DrawString("" + x--, font, new SolidBrush(Color.Red), 5, 1);

                        if (x < 0) {
                            x = 0;
                            mode++;
                        }

                        Thread.Sleep((int)(500 - (DateTime.Now.Ticks - t1)/10000));
                        break;

                    case 1:
                        graphic.DrawEllipse(new Pen(Color.White), 8 - x, 8 - x, 2 * x, 2 * x);
                        
                        x++;

                        if (x == 16) {
                            mode++;
                            x = 16;
                        }
                        break;
                    case 2:
                        graphic.DrawString(Text, font, new SolidBrush(Color.Blue), x--, 1);

                        if (x < Text.Length * -6) {
                            x = 9;
                            mode = 0;
                        }
                        Thread.Sleep((int)(100 - (DateTime.Now.Ticks - t1) / 10000));
                        break;
                }

                graphic.Flush();
            }
        }

        private static void Graphics_OnFlushEvent(System.IntPtr hdc, byte[] data) => DrawBuffer(data);

        static int RemapLedMatrix(int index) {
            var d1 = index / 16;
            var d2 = index % 16;
            var idx = index;

            if ((d1) % 2 == 0) {
                idx = ((d1 + 1) * 16 - 1) - d2;

            }

            return idx;
        }

        static void DrawBuffer(byte[] data) {
            var x = 0;
            for (var i = 0; i < data.Length; i += 2) {
                var color16 = (ushort)((data[i + 1] << 8) | data[i]);

                var r = (byte)(((color16 >> 11) & 0x1F) << 3);
                var g = (byte)(((color16 >> 5) & 0x3F) << 2);
                var b = (byte)(((color16 >> 0) & 0x1F) << 3);

                leds.SetColor(RemapLedMatrix(x), r, g, b);
                x++;
            }

            leds.Flush();
        }
    }
}
