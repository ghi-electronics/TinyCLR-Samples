using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Drivers;

using GHIElectronics.TinyCLR.Pins;

using GHIElectronics.TinyCLR.Drawing;

namespace Matrix
{
    class Program
    {
        static int WIDTH = 20*8;
        static int HEIGHT = 8;
        //static byte[] vram = new byte[WIDTH*HEIGHT / 8];
        static byte[] line = new byte[WIDTH / 8];
        static Max72197221 ledchip;
        public static void Flush(byte[] vram) {
            for (var x = 0; x < 8; x++) {
                Array.Copy(vram, x * line.Length, line, 0, line.Length);
                ledchip.DisplayLine(7-x, line);
            }
        }
        /*public static void SetPixel(int x, int y, bool color) {
            if (x < 0 || x > WIDTH - 1 || y < 0 || y > HEIGHT - 1)
                return;
            y = 7 - y;
            if(color)
                vram[(x / 8) + (y * line.Length)] |= (byte)(1 << x % 8);
            else
                vram[(x / 8) + (y * line.Length)] &= (byte)(~(1 << x % 8));

        }*/

        private static void Main()
        {
            var chipSelect = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PD3);

            var settings = new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.None,
                //ChipSelectLine = SC20100.GpioPin.PD3,
                Mode = SpiMode.Mode0,
                ClockFrequency = 4_000_000,       //4Mhz
                DataBitLength = 8,
            };


            var controller = SpiController.FromName(SC20100.SpiBus.Spi3);
            var device = controller.GetDevice(settings);
            //byte[] data = new byte[2];
            //data[0] = 9;
            //data[1] = 0x0f;
            //device.Write(data)



            ledchip = new Max72197221(chipSelect, device);
            ledchip.SetDisplayTest(Max72197221.DisplayTestRegister.DisplayTestMode);
            Thread.Sleep(100);
            ledchip.SetDisplayTest(Max72197221.DisplayTestRegister.NormalOperation);
            Thread.Sleep(100);
            
            for (int x = 0; x < 20; x++)
                ledchip.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            for(int x = 0; x < 20; x++)
                ledchip.SetDigitScanLimit(7);
            for (int x = 0; x < 20; x++)
                ledchip.SetIntensity(0x07);
            for (int x = 0; x < 20; x++)
                ledchip.Shutdown(Max72197221.ShutdownRegister.NormalOperation);

            int v = 100;
            BasicDrawing pen = new BasicDrawing(WIDTH, HEIGHT);
            pen.ClearScreen();
            

            while (true) {
                pen.ClearScreen();
                pen.DrawText(v--, 0, "Stay at home!!!");
                Flush(pen.vram);
                if (v < -100) v = 100;
                Thread.Sleep(50);
            }
          
            
        }
        
    }
}