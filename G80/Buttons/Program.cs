using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Buttons {
    class Program {
        static GpioController gpio = GpioController.GetDefault();
        static GpioPin LED1 = gpio.OpenPin(G80.GpioPin.PA0);
        static void Main() {
            var BTN1 = gpio.OpenPin(G80.GpioPin.PA0);
            BTN1.SetDriveMode(GpioPinDriveMode.InputPullUp);
            LED1.SetDriveMode(GpioPinDriveMode.Output);

            // Call this event when the button is pressed
            BTN1.ValueChanged += BTN1_ValueChanged;

            // Sleep forever, low power!
            Thread.Sleep(Timeout.Infinite);
        }

        private static void BTN1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge == GpioPinEdge.FallingEdge)
                LED1.Write(GpioPinValue.Low);
            else
                LED1.Write(GpioPinValue.High);
        }
    }
}
