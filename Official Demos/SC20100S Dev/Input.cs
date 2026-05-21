using System;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Threading;

namespace Demos {
    public static class Input {
        public static class Button {
            private static GpioPin buttonLeft;
            private static GpioPin buttonRight;
            private static GpioPin buttonCenter;

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

                buttonLeft.ValueChanged += (s, e) =>
                    Program.MainApp.InputProvider.RaiseButton(HardwareButton.Left, true, DateTime.UtcNow);
                buttonRight.ValueChanged += (s, e) =>
                    Program.MainApp.InputProvider.RaiseButton(HardwareButton.Right, true, DateTime.UtcNow);

                // Center button is read via timer poll rather than edge IRQ —
                // ValueChanged doesn't have a built-in debounce property here
                // and we want a clean rising-edge equivalent.
                StartCenterButtonPoll();
            }

            private static DispatcherTimer centerPollTimer;
            private static bool centerPressed;

            private static void StartCenterButtonPoll() {
                centerPollTimer = new DispatcherTimer();
                centerPollTimer.Tick += CenterPollTimer_Tick;
                centerPollTimer.Interval = TimeSpan.FromMilliseconds(50);
                centerPollTimer.Start();
            }

            private static void CenterPollTimer_Tick(object sender, EventArgs e) {
                var pressedNow = buttonCenter.Read() == GpioPinValue.Low;
                if (pressedNow && !centerPressed) {
                    centerPressed = true;
                }
                else if (!pressedNow && centerPressed) {
                    centerPressed = false;
                    Program.MainApp.InputProvider.RaiseButton(HardwareButton.Select, true, DateTime.UtcNow);
                }
            }
        }
    }
}
