using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGrove
{
    class FourDigitDisplay
    {
        private const int AddrAuto = 0x40;
        private const int AddrFixed = 0x44;
        private const int AddrStart = 0xc0;

        private readonly GpioPin _pinClock;
        private readonly GpioPin _pinData;
        private bool _bPoint;
        private int _cmdDisplayCtrl;
        private int _brightness;

        /// <summary>
        /// Brightness enum value, name are from pwm value: i.e. Pw02 stand for a pwm with a pulse width of 02/16.
        /// </summary>
        public enum Brightness
        {
            /// <summary>
            /// Darkest value
            /// </summary>
            Pw01,
            Pw02,
            /// <summary>
            /// Typical value
            /// </summary>
            Pw04,
            Pw10,
            Pw11,
            Pw12,
            Pw13,
            /// <summary>
            /// Brightest value
            /// </summary>
            Pw14
        }

        /// <summary>
        /// Select digit number to display.
        /// </summary>
        public enum Digit { First, Second, Third, Fourth }

        /// <summary>
        /// Value to use to display a blank digit
        /// </summary>
        public const int Blank = 0x10;


        /// <summary>
        /// Digits to display from 0 to 9, and A-F, value 0x10 is blank value.
        /// </summary>
        private static readonly byte[] Digits = new byte[] { 0x3f, 0x06, 0x5b, 0x4f, 0x66, 0x6d, 0x7d, 0x07, 0x7f, 0x6f, 0x77, 0x7c, 0x39, 0x5e, 0x79, 0x71, 0x00 };

        /// <summary>
        /// Create a FourDigitDisplay controller 
        /// </summary>
        /// <param name="gpioPinClockNumber">Value of pin for clock signal (yellow cable).</param>
        /// <param name="gpioPinDataNumber">Value of pin for data signal (white cable).</param>
        public FourDigitDisplay(int gpioPinClockNumber, int gpioPinDataNumber)
        {
            _pinClock = GpioController.GetDefault().OpenPin(gpioPinClockNumber);
            _pinClock.SetDriveMode(GpioPinDriveMode.Output);
            _pinData = GpioController.GetDefault().OpenPin(gpioPinDataNumber);
            _pinData.SetDriveMode(GpioPinDriveMode.Output);
            _bPoint = false;
            InitDisplay();
        }

        /// <summary>
        /// Set brightness for display, it takes effect on next display.
        /// </summary>
        /// <param name="brightness"> Use value of <c>Brightness</c> enum.</param>
        public void SetBrightness(Brightness brightness)
        {
            _brightness = (int)brightness;
            _cmdDisplayCtrl = 0x88 + _brightness;
        }

        /// <summary>
        /// Indicates to display or not the two points between digit.
        /// </summary>
        /// <param name="bPoint">true to display points,false otherwise.</param>
        public void SetPoint(bool bPoint)
        {
            _bPoint = bPoint;
        }

        private void InitDisplay()
        {
            // Set display ON and set default brightness
            SetBrightness(Brightness.Pw04);
            SendStartSignal();
            WriteValue(_cmdDisplayCtrl);
            SendStopSignal();

            // Clear display
            Display(0x00, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Display value for each digit
        /// </summary>
        /// <param name="firstDigit">Value to display for first digit (0x10 to blank).</param>
        /// <param name="secondDigit">Value to display for second digit (0x10 to blank).</param>
        /// <param name="thirdDigit">Value to display for third digit (0x10 to blank).</param>
        /// <param name="fourthDigit">Value to display for fourth digit (0x10 to blank).</param>
        public void Display(int firstDigit, int secondDigit, int thirdDigit, int fourthDigit)
        {
            SendStartSignal();
            WriteValue(AddrAuto);
            SendStopSignal();

            SendStartSignal();
            WriteValue(AddrStart);
            WriteValue(Digits[firstDigit] + (_bPoint ? 0x80 : 0x00));
            WriteValue(Digits[secondDigit] + (_bPoint ? 0x80 : 0x00));
            WriteValue(Digits[thirdDigit] + (_bPoint ? 0x80 : 0x00));
            WriteValue(Digits[fourthDigit] + (_bPoint ? 0x80 : 0x00));
            SendStopSignal();
            SendStartSignal();
            WriteValue(_cmdDisplayCtrl);
        }

        public void Display(Digit digit, int value)
        {
            SendStartSignal();
            WriteValue(AddrFixed);
            SendStopSignal();

            SendStartSignal();
            WriteValue(AddrStart + (int)digit);
            WriteValue(Digits[value] + (_bPoint ? 0x80 : 0x00));
            SendStopSignal();
            SendStartSignal();
            WriteValue(_cmdDisplayCtrl);
        }


        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool WriteValue(int value)
        {
            // Send data 
            for (int i = 0; i < 8; i++)
            {
                _pinClock.Write(GpioPinValue.Low);
                Wait();
                _pinData.Write(((value & (1 << i)) >> i) == 0 ? GpioPinValue.Low : GpioPinValue.High);
                Wait();
                _pinClock.Write(GpioPinValue.High);
                Wait();
            }

            // Wait for ACK
            _pinClock.Write(GpioPinValue.Low);
            Wait();
            _pinData.SetDriveMode(GpioPinDriveMode.Input);
            _pinClock.Write(GpioPinValue.High);
            Wait();
            bool ack = _pinData.Read() == GpioPinValue.Low;
            _pinData.SetDriveMode(GpioPinDriveMode.Output);
            return ack;
        }

        private void Wait()
        {
            Thread.Sleep(1);// ideally 5 microseconds but impossible in managed code
        }

        private void SendStartSignal()
        {
            _pinClock.Write(GpioPinValue.High);
            _pinData.Write(GpioPinValue.High);
            Wait();
            _pinData.Write(GpioPinValue.Low);
            _pinClock.Write(GpioPinValue.Low);
            Wait();
        }

        private void SendStopSignal()
        {
            _pinClock.Write(GpioPinValue.Low);
            _pinData.Write(GpioPinValue.Low);
            Wait();
            _pinClock.Write(GpioPinValue.High);
            _pinData.Write(GpioPinValue.High);
            Wait();
        }
    }
}
