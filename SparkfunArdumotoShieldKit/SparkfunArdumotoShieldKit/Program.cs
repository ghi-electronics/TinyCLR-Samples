using System.Threading;

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

namespace SparkfunArdumotoShieldKit {
    class Program {
        static void Main() {
            var gpio = GpioController.GetDefault();
            var dirA = gpio.OpenPin(FEZ.GpioPin.D2);
            var dirB = gpio.OpenPin(FEZ.GpioPin.D4);
            dirA.SetDriveMode(GpioPinDriveMode.Output);
            dirB.SetDriveMode(GpioPinDriveMode.Output);

            var pwm1 = PwmController.FromName(FEZ.PwmChannel.Controller1.Id);
            var pwm3 = PwmController.FromName(FEZ.PwmChannel.Controller3.Id);
            pwm1.SetDesiredFrequency(5000);
            pwm3.SetDesiredFrequency(5000);

            var pwmA = pwm1.OpenChannel(FEZ.PwmChannel.Controller1.D3);
            var pwmB = pwm3.OpenChannel(FEZ.PwmChannel.Controller3.D11);
            pwmA.Start();
            pwmB.Start();

            // reverse direction every one second!
            // Do not foget the shield needs power. Thsi can come from VIN, meaning plug a power pack into your *duino board.
            pwmB.SetActiveDutyCyclePercentage(0.9);

            while(true) {
                dirA.Write(GpioPinValue.High);
                dirB.Write(GpioPinValue.High);
                Thread.Sleep(1000);

                // Change speed
                pwmA.SetActiveDutyCyclePercentage(0.9);

                dirA.Write(GpioPinValue.Low);
                dirB.Write(GpioPinValue.Low);
                Thread.Sleep(1000);

                // Cahnge speed
                pwmA.SetActiveDutyCyclePercentage(0.5);
            }
        }
    }
}
