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
            static GpioPin buttonUp;
            static GpioPin buttonA;
            static GpioPin buttonB;

            public static void InitializeButtons() {
                var gpioController = GpioController.GetDefault();

                buttonLeft = gpioController.OpenPin(SC20100.GpioPin.PE3);
                buttonRight = gpioController.OpenPin(SC20100.GpioPin.PB7);
                buttonCenter = gpioController.OpenPin(SC20100.GpioPin.PA1);
                buttonUp = gpioController.OpenPin(SC20100.GpioPin.PE4);
                buttonA = gpioController.OpenPin(SC20100.GpioPin.PE5);
                buttonB = gpioController.OpenPin(SC20100.GpioPin.PE6);

                buttonLeft.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonRight.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonCenter.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonUp.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonA.SetDriveMode(GpioPinDriveMode.InputPullUp);
                buttonB.SetDriveMode(GpioPinDriveMode.InputPullUp);

                buttonLeft.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                buttonRight.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                buttonCenter.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                buttonUp.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                buttonA.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                buttonB.DebounceTimeout = TimeSpan.FromMilliseconds(50);

                buttonLeft.ValueChangedEdge = GpioPinEdge.RisingEdge;
                buttonRight.ValueChangedEdge = GpioPinEdge.RisingEdge;
                buttonCenter.ValueChangedEdge = GpioPinEdge.RisingEdge;                
                buttonUp.ValueChangedEdge = GpioPinEdge.RisingEdge;                
                buttonA.ValueChangedEdge = GpioPinEdge.RisingEdge;                
                buttonB.ValueChangedEdge = GpioPinEdge.RisingEdge;                

                buttonLeft.ValueChanged += ButtonLeft_ValueChanged;
                buttonRight.ValueChanged += ButtonRight_ValueChanged;
                buttonCenter.ValueChanged += ButtonCenter_ValueChanged;                
                buttonUp.ValueChanged += ButtonCenter_ValueChanged;                
                buttonA.ValueChanged += ButtonCenter_ValueChanged;                
                buttonB.ValueChanged += ButtonCenter_ValueChanged;                
            }

            public static void DeinitializeButtons() {

                buttonLeft.ValueChanged -= ButtonLeft_ValueChanged;
                buttonRight.ValueChanged -= ButtonRight_ValueChanged;
                buttonCenter.ValueChanged -= ButtonCenter_ValueChanged;
                buttonUp.ValueChanged -= ButtonCenter_ValueChanged;
                buttonA.ValueChanged -= ButtonCenter_ValueChanged;
                buttonB.ValueChanged -= ButtonCenter_ValueChanged;

                buttonLeft.Dispose();
                buttonRight.Dispose();
                buttonCenter.Dispose();
                buttonUp.Dispose();
                buttonA.Dispose();
                buttonB.Dispose();
            }


            private static void ButtonRight_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Program.MainApp.InputProvider.RaiseButton(GHIElectronics.TinyCLR.UI.Input.HardwareButton.Right, true, DateTime.UtcNow);

            private static void ButtonLeft_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Program.MainApp.InputProvider.RaiseButton(GHIElectronics.TinyCLR.UI.Input.HardwareButton.Left, true, DateTime.UtcNow);

            private static void ButtonCenter_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Program.MainApp.InputProvider.RaiseButton(GHIElectronics.TinyCLR.UI.Input.HardwareButton.Select, true, DateTime.UtcNow);

        }
    }
}
