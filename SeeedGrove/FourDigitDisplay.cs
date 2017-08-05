using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGrove
{
    class FourDigitDisplay
    {
        GpioPin PinClock;
        GpioPin PinData;

        public static readonly byte[] Digits = new byte[] { 0x3f, 0x06, 0x5b, 0x4f, 0x66, 0x6d, 0x7d, 0x07, 0x7f, 0x6f };

        public FourDigitDisplay(int gpioPinClockNumber, int gpioPinDataNumber)
        {
            PinClock = GpioController.GetDefault().OpenPin(gpioPinClockNumber);
            PinClock.SetDriveMode(GpioPinDriveMode.Output);
            PinData = GpioController.GetDefault().OpenPin(gpioPinDataNumber);
            PinData.SetDriveMode(GpioPinDriveMode.Output);
            InitDisplay();
        }

        private void InitDisplay()
        {
            // Set display ON and set default brightness
            SendStartSignal();
            WriteValue(0x8c);
            SendStopSignal();

            // Clear display
            Write(0x00,0x00,0x00,0x00);
        }

        public void Write(int firstDigit, int secondDigit, int thirdDigit, int fourthDigit)
        {
            SendStartSignal();
            WriteValue(0x40);
            SendStopSignal();

            SendStartSignal();
            WriteValue(0xC0);
            WriteValue(firstDigit);
            WriteValue(secondDigit);
            WriteValue(thirdDigit);
            WriteValue(fourthDigit);
            SendStopSignal();
        }

        private bool WriteValue(int value)
        {
            // Send data 
            for (int i = 0; i < 8; i++)
            {
                PinClock.Write(GpioPinValue.Low);
                Wait();
                PinData.Write( ((value & (1 << i))>>i)==0?GpioPinValue.Low : GpioPinValue.High);
                Wait();
                PinClock.Write(GpioPinValue.High);
                Wait();
            }

            // Wait for ACK
            PinClock.Write(GpioPinValue.Low);
            Wait();
            PinData.SetDriveMode(GpioPinDriveMode.Input);
            PinClock.Write(GpioPinValue.High);
            Wait();
            bool ack = PinData.Read() == GpioPinValue.Low;
            PinData.SetDriveMode(GpioPinDriveMode.Output);
            return ack;
        }

        private void Wait()
        {
            Thread.Sleep(1);// ideally 5 microseconds but impossible in managed code
        }

        private void SendStartSignal()
        {
            PinClock.Write(GpioPinValue.High);
            PinData.Write(GpioPinValue.High);
            Wait();
            PinData.Write(GpioPinValue.Low);
            PinClock.Write(GpioPinValue.Low);
            Wait();
        }

        private void SendStopSignal()
        {
            PinClock.Write(GpioPinValue.Low);
            PinData.Write(GpioPinValue.Low);
            Wait();
            PinClock.Write(GpioPinValue.High);
            PinData.Write(GpioPinValue.High);
            Wait();
        }

    }

}
