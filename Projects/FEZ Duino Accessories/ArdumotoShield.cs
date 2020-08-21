// https://www.sparkfun.com/products/14129
// H bridge for motor control

using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;

namespace GHIElectronics.TinyCLR.Drivers.Shield {
    class ArdumotoShield {
        private PwmChannel speedA, speedB;
        private GpioPin dirA, dirB;
        public ArdumotoShield(PwmChannel speedA, PwmChannel speedB, GpioPin dirA, GpioPin dirB) {
            this.dirA = dirA;
            this.dirB = dirB;
            this.speedA = speedA;
            this.speedB = speedB;

            this.dirA.SetDriveMode(GpioPinDriveMode.Output);
            this.dirB.SetDriveMode(GpioPinDriveMode.Output);

            this.speedA.Controller.SetDesiredFrequency(5_000);
            this.speedB.Controller.SetDesiredFrequency(5_000);

        }
        // range 0 to 1. 0.5 is half speed
        public void SetSpeed(double a, double b) {
            if (a > 0) {
                this.dirA.Write(GpioPinValue.High);
            }
            else {
                this.dirA.Write(GpioPinValue.Low);
                a *= -1;
            }
            this.speedA.SetActiveDutyCyclePercentage(a);

            if (b > 0) {
                this.dirB.Write(GpioPinValue.High);
            }
            else {
                this.dirB.Write(GpioPinValue.Low);
                b *= -1;
            }
            this.speedB.SetActiveDutyCyclePercentage(b);
        }
    }
}
