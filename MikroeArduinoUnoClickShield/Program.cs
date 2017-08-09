using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;
using System.Threading;

namespace MikroeArduinoUnoClickShield {
    public class Program {
        public static void Main() {
            var spiSettings = new SpiConnectionSettings(FEZ.GpioPin.D10) {
                ClockFrequency = 12_000_000,
                DataBitLength = 8,
                Mode = SpiMode.Mode0
            };

            var gpioController = GpioController.GetDefault();
            var mr = gpioController.OpenPin(FEZ.GpioPin.A3);
            var spi = SpiDevice.FromId(FEZ.SpiBus.Spi1, spiSettings);

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
                data[2] = (byte)(count >> 16);
                data[3] = (byte)(count >> 24);

                spi.Write(data);

                Thread.Sleep(20);
            }
        }
    }
}