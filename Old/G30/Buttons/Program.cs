using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Buttons {
    class Program {
        static GpioPin led1;
        static void Main() {
            var gpio = GpioController.GetDefault();
            led1 = gpio.OpenPin(G30.GpioPin.PC7);
            var ldr0 = gpio.OpenPin(G30.GpioPin.PA15);
            ldr0.SetDriveMode(GpioPinDriveMode.InputPullUp);
            led1.SetDriveMode(GpioPinDriveMode.Output);

            ldr0.DebounceTimeout = new System.TimeSpan(0, 0, 0, 0, 1);
            // Call this event when the button is pressed
            ldr0.ValueChanged += ldr0_ValueChanged;

            // you can also read the button directly
            var state = ldr0.Read();

            // Sleep forever, low power!
            Thread.Sleep(Timeout.Infinite);
        }

        private static void ldr0_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge == GpioPinEdge.FallingEdge)
                led1.Write(GpioPinValue.High);
            else
                led1.Write(GpioPinValue.Low);
        }
    }
}
