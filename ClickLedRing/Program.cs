using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;

namespace ClickLedRing {
    class Program {
        static void Main() {
            var settings = new SpiConnectionSettings(FEZ.GpioPin.D10) {
                ClockFrequency = 12 * 1000 * 1000,
                DataBitLength = 8,
                Mode = SpiMode.Mode0

            };

            var spi = SpiDevice.FromId(BrainPad.SpiBus.Spi1, settings);
            var mr = GpioController.GetDefault().OpenPin(FEZ.GpioPin.A3);
            mr.SetDriveMode(GpioPinDriveMode.Output);
            mr.Write(GpioPinValue.High);
            var count = 1;
            var data = new byte[4];


            while (true) {
                count <<= 1;
                if (count == 0)
                    count = 1;

                data[0] = (byte)(count >> 0);
                data[1] = (byte)(count >> 8);
                data[2] = (byte)(count >> 8 + 8);
                data[3] = (byte)(count >> 8 + 8 + 8);
                spi.Write(data);

                Thread.Sleep(20);

            }
        }
    }
}
