using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public class Input {
        public static class Touch {            

            static FT5xx6Controller touch;            

            public static void InitializeTouch() {
                var i2cController = I2cController.FromName(SC20260.I2cBus.I2c1);

                var settings = new I2cConnectionSettings(0x38) {
                    BusSpeed = 100000,
                    AddressFormat = I2cAddressFormat.SevenBit,
                };

                var i2cDevice = i2cController.GetDevice(settings);

                var gpioController = GpioController.GetDefault();
                var interrupt = gpioController.OpenPin(SC20260.GpioPin.PJ14);

                touch = new FT5xx6Controller(i2cDevice, interrupt);
                touch.TouchDown += Touch_TouchDown;
                touch.TouchUp += Touch_TouchUp;
            }

            private static void Touch_TouchUp(FT5xx6Controller sender, TouchEventArgs e) => Program.MainApp.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Up, System.DateTime.Now);
            private static void Touch_TouchDown(FT5xx6Controller sender, TouchEventArgs e) => Program.MainApp.InputProvider.RaiseTouch(e.X, e.Y, GHIElectronics.TinyCLR.UI.Input.TouchMessages.Down, System.DateTime.Now);
        }

        public static class Button {
            static GpioPin buttonLeft;
            static GpioPin buttonRight;
            static GpioPin buttonCenter;

            public static void InitializeButtons() {
                var gpioController = GpioController.GetDefault();

                buttonLeft = gpioController.OpenPin(SC20100.GpioPin.PE3);
                buttonRight = gpioController.OpenPin(SC20100.GpioPin.PD7);
                buttonCenter = gpioController.OpenPin(SC20100.GpioPin.PB7);

                buttonLeft.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonRight.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonCenter.SetDriveMode(GpioPinDriveMode.InputPullUp);

                buttonLeft.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                buttonRight.DebounceTimeout = TimeSpan.FromMilliseconds(50);

                buttonLeft.ValueChangedEdge = GpioPinEdge.RisingEdge;
                buttonRight.ValueChangedEdge = GpioPinEdge.RisingEdge;

                buttonLeft.ValueChanged += ButtonLeft_ValueChanged;
                buttonRight.ValueChanged += ButtonRight_ValueChanged;

                CreateClockTimer();
            }


            private static void ButtonRight_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Program.MainApp.InputProvider.RaiseButton(GHIElectronics.TinyCLR.UI.Input.HardwareButton.Right, true, DateTime.UtcNow);

            private static void ButtonLeft_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Program.MainApp.InputProvider.RaiseButton(GHIElectronics.TinyCLR.UI.Input.HardwareButton.Left, true, DateTime.UtcNow);

            private static void ButtonCenter_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Program.MainApp.InputProvider.RaiseButton(GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select, true, DateTime.UtcNow);

            private static uint buttonLeftMask = 0;
            private static uint buttonRightMask = 0;

            public static bool IsButtonLeftPressed() => (buttonLeftMask & 1) > 0;
            public static bool IsButtonRightPressed() => (buttonRightMask & 1) > 0;

            public static void ClearButtonLeftState() => buttonLeftMask = 0;
            public static void ClearButtonRightState() => buttonRightMask = 0;

            static private DispatcherTimer clockTimer;

            static private void CreateClockTimer() {
                clockTimer = new DispatcherTimer();

                clockTimer.Tick += ClockTimer_Tick;
                clockTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                clockTimer.Start();
            }

            static bool isButtonCenterPressed = false;

            static private void ClockTimer_Tick(object sender, EventArgs e) {
                if (buttonCenter.Read() == GpioPinValue.Low) {
                    if (isButtonCenterPressed == false) {
                        isButtonCenterPressed = true;
                    }
                }

                if (buttonCenter.Read() == GpioPinValue.High) {
                    if (isButtonCenterPressed == true) {

                        isButtonCenterPressed = false;

                        ButtonCenter_ValueChanged(buttonCenter, null);
                    }
                }
            }
        }
    }
}
