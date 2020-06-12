using GHIElectronics.TinyCLR.Drivers.Waveshare;
using GHIElectronics.TinyCLR.Pins;
using System.Drawing;
using System.Threading;

namespace Sc20100sDevEpaperTest {
    class Program {
        private static E_Ink_13368 eInk;

        static void Main() {
            //Tested on SCM20100S Dev Rev B using Waveshare 2.13inch e-Paper HAT (B).
            //SPI 6
            //CS#   = PE1 (Active L)
            //D/C#  = PE0 (H for data, L for command)
            //RST#  = PB8 (L for reset)
            //BUSY# = PB9 (L for busy).
            //Configured for 4 line SPI.

            var screenWidth = 212;
            var screenHeight = 104;

            eInk = new E_Ink_13368(SC20100.SpiBus.Spi6, SC20100.GpioPin.PE1, SC20100.GpioPin.PE0, SC20100.GpioPin.PB8, SC20100.GpioPin.PB9);
            eInk.RefreshDisplay(); //Instantiating display clears the buffer, but doesn't refresh the display.

            var screen = Graphics.FromImage(new Bitmap(screenWidth, screenHeight));

            Graphics.OnFlushEvent += Graphics_OnFlushEvent;

            screen.Clear();

            var logo1 = Resource1.GetBitmap(Resource1.BitmapResources.GhiLogo);
            var logo2 = Resource1.GetBitmap(Resource1.BitmapResources.GhiLogo2);
            var logo3 = Resource1.GetBitmap(Resource1.BitmapResources.GhiLogo3);

            while (true) {
                screen.DrawImage(logo1, 0, 0);
                Thread.Sleep(5000);
                screen.Flush();

                screen.DrawImage(logo2, 0, 0);
                Thread.Sleep(5000);
                screen.Flush();

                screen.DrawImage(logo3, 0, 0);
                Thread.Sleep(5000);
                screen.Flush();
            }
        }

        private static void Graphics_OnFlushEvent(System.IntPtr hdc, byte[] buffer) {
            eInk.DrawBuffer(buffer);
            eInk.RefreshDisplay();
        }
    }
}
