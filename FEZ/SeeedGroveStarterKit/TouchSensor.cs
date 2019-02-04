using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGroveStarterKit {
    public class TouchSensor {
        GpioPin Pin;
        public TouchSensor(int PinNumber) {
            this.Pin = GpioController.GetDefault().OpenPin(PinNumber);
            this.Pin.Write(GpioPinValue.Low);
            this.Pin.SetDriveMode(GpioPinDriveMode.Input);
            this.Pin.ValueChanged += this.Pin_ValueChanged;
        }

        public bool IsTouched() => this.Pin.Read() == GpioPinValue.High;
        
        /// <summary>
        /// The signature of button events.
        /// </summary>
        public delegate void TouchEventHandler();

        /// <summary>
        /// The event raised when a button is released.
        /// </summary>
        public event TouchEventHandler Touched;
        public event TouchEventHandler Untouched;
        private void Pin_ValueChanged(object sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge == GpioPinEdge.RisingEdge)
                Touched?.Invoke();
            else
                Untouched?.Invoke();
        }

    }
}
