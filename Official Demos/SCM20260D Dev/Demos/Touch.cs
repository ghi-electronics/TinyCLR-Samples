using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.Pins;

namespace Demos {
    static class Touch {
        const int TOUCH_IRQ = SC20260.GpioPin.PJ14;

        static FT5xx6Controller touch;

        public static void InitializeTouch() {
            var i2cController = I2cController.GetDefault();

            var settings = new I2cConnectionSettings(0x38) {// the slave's address
                BusSpeed = 100000,
                AddressFormat = I2cAddressFormat.SevenBit,
            };

            var i2cDevice = i2cController.GetDevice(settings);

            var interrupt = GpioController.GetDefault().OpenPin(TOUCH_IRQ);

            touch = new FT5xx6Controller(i2cDevice, interrupt);
            touch.TouchDown += Touch_TouchDown;
            touch.TouchUp += Touch_TouchUp;
        }

        private static void Touch_TouchUp(FT5xx6Controller sender, TouchEventArgs e) => Program.MainApp.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Up, System.DateTime.Now);
        private static void Touch_TouchDown(FT5xx6Controller sender, TouchEventArgs e) => Program.MainApp.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Down, System.DateTime.Now);

    }
}
