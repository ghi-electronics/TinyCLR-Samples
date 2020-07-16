using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public class Input {
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

            static void ThreadTimer() {
                while (true) {
                    Thread.Sleep(50);

                    if (buttonCenter == null) {
                        continue;
                    }

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

            static private void CreateClockTimer() => new Thread(ThreadTimer).Start();

            static bool isButtonCenterPressed = false;
        }
    }
}
