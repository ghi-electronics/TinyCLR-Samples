using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Buttons {
    class Program {
        static GpioPin led1;
        static void Main() {
            var gpio = GpioController.GetDefault();
            led1 = gpio.OpenPin(G80.GpioPin.PE14);
            var btn1 = gpio.OpenPin(G80.GpioPin.PE0);
            btn1.SetDriveMode(GpioPinDriveMode.InputPullUp);
            led1.SetDriveMode(GpioPinDriveMode.Output);

            btn1.DebounceTimeout = new System.TimeSpan(0, 0, 0, 0, 1);
            // Call this event when the button is pressed
            btn1.ValueChanged += btn1_ValueChanged;

            // you can also read the button directly
            var state = btn1.Read();

            // Sleep forever, low power!
            Thread.Sleep(Timeout.Infinite);
        }

        private static void btn1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge == GpioPinEdge.FallingEdge)
                led1.Write(GpioPinValue.High);
            else
                led1.Write(GpioPinValue.Low);
        }
    }
}
