// Todo: Add fault and sleep support

using System;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;

namespace Mikro.Click {

    public class DCMotor {
        private GpioPin select1;
        private GpioPin select2;
        private PwmChannel pwm;
        public DCMotor(GpioPin sel1, GpioPin sel2, PwmChannel pwm) {
            this.select1 = sel1;
            this.select2 = sel2;
            this.pwm = pwm;

            this.select1.SetDriveMode(GpioPinDriveMode.Output);
            this.select2.SetDriveMode(GpioPinDriveMode.Output);
            this.select1.Write(GpioPinValue.Low);
            this.pwm.Controller.SetDesiredFrequency(10_000);
            this.pwm.SetActiveDutyCyclePercentage(0);
            this.pwm.Start();
        }
        public void Set (double speed) {
            if (Math.Abs(speed) > 100)
                throw new Exception("Wrong Speed");
            if (speed>0) 
                this.select2.Write(GpioPinValue.High);
            else {
                this.select2.Write(GpioPinValue.Low);
                //speed = 100 - speed;
            }

            this.pwm.SetActiveDutyCyclePercentage(Math.Abs(speed)/100);
        }
    }
}


