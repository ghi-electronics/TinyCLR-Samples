using GHIElectronics.TinyCLR.Devices.Gpio;

namespace SeeedGroveStarterKit {
    public class TouchSensor {
        GpioPin Pin;
        public TouchSensor(int PinNumber) {
            Pin = GpioController.GetDefault().OpenPin(PinNumber);
            Pin.Write(GpioPinValue.Low);
            Pin.SetDriveMode(GpioPinDriveMode.Input);
            Pin.ValueChanged += Pin_ValueChanged;
        }

        public bool IsTouched() {
            return Pin.Read() == GpioPinValue.High;
        }


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
