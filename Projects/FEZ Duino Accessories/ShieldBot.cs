//https://www.seeedstudio.com/Shield-Bot-p-1380.html
// 5 front reflectors on A0 to A4, digital
// right motor D8 and D11 for direction and PWM speed on 9
// left motor D12 and D13 for direction and PWM speed on 10

using System;
using System.Collections;
using System.Text;
using System.Threading;


using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;

namespace FEZ_Duino {
    class ShieldBot {
        private PwmChannel speedLeft, speedRight;
        private GpioPin dirL1, dirL2, dirR1, dirR2;

        public ShieldBot(PwmChannel speedLeft, PwmChannel speedRight, GpioPin dirL1, GpioPin dirL2, GpioPin dirR1, GpioPin dirR2) {
            this.dirL1 = dirL1;
            this.dirL1 = dirL1;
            this.speedLeft = speedLeft;
            this.speedRight = speedRight;

            this.dirL1.SetDriveMode(GpioPinDriveMode.Output);
            this.dirR1.SetDriveMode(GpioPinDriveMode.Output);
            this.dirL2.SetDriveMode(GpioPinDriveMode.Output);
            this.dirR2.SetDriveMode(GpioPinDriveMode.Output);

            this.speedLeft.Controller.SetDesiredFrequency(5_000);
            this.speedRight.Controller.SetDesiredFrequency(5_000);
        }
        // range 0 to 1. 0.5 is half speed
        public void SetSpeed(double left, double right) {
            if (left > 0) {
                this.dirL1.Write(GpioPinValue.High);
                this.dirL2.Write(GpioPinValue.Low);
            }
            else {
                this.dirL1.Write(GpioPinValue.Low);
                this.dirL2.Write(GpioPinValue.High);
                left *= -1;
            }
            this.speedLeft.SetActiveDutyCyclePercentage(left);

            if (right > 0) {
                this.dirR1.Write(GpioPinValue.High);
                this.dirR2.Write(GpioPinValue.Low);
            }
            else {
                this.dirR1.Write(GpioPinValue.Low);
                this.dirR1.Write(GpioPinValue.High);
                right *= -1;
            }
            this.speedRight.SetActiveDutyCyclePercentage(right);
        }
    }

}
