using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGroveStarterKit {
    public class Button {
        GpioPin Pin;

        public Button(int GpioPinNumber) {
            Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            Pin.Write(GpioPinValue.Low);
            Pin.SetDriveMode(GpioPinDriveMode.Input);
            Pin.ValueChanged += Pin_ValueChanged;
        }

        public bool IsPressed() {
            return Pin.Read() == GpioPinValue.High;
        }
        /// <summary>
        /// The signature of button events.
        /// </summary>
        public delegate void ButtonEventHandler();

        /// <summary>
        /// The event raised when a button is released.
        /// </summary>
        public event ButtonEventHandler ButtonReleased;
        public event ButtonEventHandler ButtonPressed;
        private void Pin_ValueChanged(object sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge == GpioPinEdge.RisingEdge)
                ButtonPressed?.Invoke();
            else
                ButtonReleased?.Invoke();
        }

    }
}
