using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGroveStarterKit {
    public class Button {
        GpioPin Pin;

        public Button(int GpioPinNumber) {
            this.Pin = GpioController.GetDefault().OpenPin(GpioPinNumber);
            this.Pin.Write(GpioPinValue.Low);
            this.Pin.SetDriveMode(GpioPinDriveMode.Input);
            this.Pin.ValueChanged += this.Pin_ValueChanged;
        }

        public bool IsPressed() {
            return this.Pin.Read() == GpioPinValue.High;
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
