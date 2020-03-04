using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.TexasInstruments.CD4017B;
using GHIElectronics.TinyCLR.Drivers.TexasInstruments.SNx4HC595;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Click_7x10R
{
    class Program
    {
        static SNx4HC595 snx4hc595;
        static CD4017B cd4017;
        static byte[][] numberArray;

        static void Main()
        {
            var latchPin = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PD3);
            var cd4017ClockPin = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PC0);
            var cd4017ResetPin = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PA15);

            snx4hc595 = new SNx4HC595(SpiController.FromName(SC20100.SpiBus.Spi3).GetDevice(SNx4HC595.GetSpiConnectionSettings()), latchPin);
            cd4017 = new CD4017B(cd4017ClockPin, cd4017ResetPin);

            InitNumberArray();

            cd4017.ResetCount();

            var mainCounter = 0;

            while (true)
            {
                var exp = DateTime.Now.Ticks + 1000 * 10000; // Count up every 1 second

                while (DateTime.Now.Ticks < exp)
                {
                    cd4017.ResetCount();

                    DrawNumber(mainCounter);
                }

                if (++mainCounter == 100)
                {
                    mainCounter = 0;
                }
            }

        }

        static void InitNumberArray()
        {
            numberArray = new byte[10][];

            // Flip X
            numberArray[0] = new byte[] { SwapEndian(0x0E), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x0E) };  //0
            numberArray[1] = new byte[] { SwapEndian(0x0E), SwapEndian(0x04), SwapEndian(0x04), SwapEndian(0x04), SwapEndian(0x04), SwapEndian(0x0C), SwapEndian(0x04) };  //1
            numberArray[2] = new byte[] { SwapEndian(0x1F), SwapEndian(0x08), SwapEndian(0x04), SwapEndian(0x02), SwapEndian(0x01), SwapEndian(0x11), SwapEndian(0x0E) };  //2
            numberArray[3] = new byte[] { SwapEndian(0x0E), SwapEndian(0x11), SwapEndian(0x01), SwapEndian(0x02), SwapEndian(0x04), SwapEndian(0x02), SwapEndian(0x1F) };  //3
            numberArray[4] = new byte[] { SwapEndian(0x02), SwapEndian(0x02), SwapEndian(0x1F), SwapEndian(0x12), SwapEndian(0x0A), SwapEndian(0x06), SwapEndian(0x02) };  //4
            numberArray[5] = new byte[] { SwapEndian(0x0E), SwapEndian(0x11), SwapEndian(0x01), SwapEndian(0x01), SwapEndian(0x1E), SwapEndian(0x10), SwapEndian(0x1F) };  //5
            numberArray[6] = new byte[] { SwapEndian(0x0E), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x1E), SwapEndian(0x10), SwapEndian(0x08), SwapEndian(0x06) };  //6
            numberArray[7] = new byte[] { SwapEndian(0x08), SwapEndian(0x08), SwapEndian(0x08), SwapEndian(0x04), SwapEndian(0x02), SwapEndian(0x01), SwapEndian(0x1F) };  //7
            numberArray[8] = new byte[] { SwapEndian(0x0E), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x0E), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x0E) };  //8
            numberArray[9] = new byte[] { SwapEndian(0x0C), SwapEndian(0x02), SwapEndian(0x01), SwapEndian(0x0F), SwapEndian(0x11), SwapEndian(0x11), SwapEndian(0x0E) };  //9
        }

        static byte SwapEndian(byte num)
        {
            var d = 0;

            for (int i = 4; i >= 0; i--)
            {
                var b = (num >> i) & 1;

                if (b != 0)
                    d |= (1 << (4 - i));
            }

            return (byte)d;
        }

        static void DrawNumber(int counter)
        {
            var buffer2 = new byte[2];

            for (int i = 0; i < 7; i++)
            {
                // Flip Y
                buffer2[0] = numberArray[counter % 10][6 - i];
                buffer2[1] = numberArray[counter / 10][6 - i];

                snx4hc595.WriteBuffer(buffer2);
                Thread.Sleep(1);

                snx4hc595.Invalidate(); // clear for next line
                cd4017.IncrementCount();
            }
        }
    }

}
