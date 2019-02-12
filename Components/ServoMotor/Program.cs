using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

using GHIElectronics.TinyCLR.Motor;

namespace ServoMotor {
    class Program {
        static void Main() {
            // Servos have 3 pins The middle one is always power. Then black or brown side is ground.
            // The remaining pin is the signal that needs PWM.

            var controller = PwmController.FromName(FEZ.PwmChannel.Controller1.Id);
            var channel = controller.OpenChannel(FEZ.PwmChannel.Controller1.D3);
            var servo = new Servo(controller, channel);

            // This is for continous servos
            // Speed is anywhere -100 to 100
            servo.ConfigureAsContinuous(false);
            var speed = 1;
            while (true) {
                servo.Set(speed);
                if ((speed += 10) > 100)
                    speed = -100;
                Thread.Sleep(100);
            }
            /*
            // This is for positional servos
            // Pos is anywhere 0 to 180
            servo.ConfigureAsPositional(false);
            var pos = 1;
            while (true) {
                servo.Set(pos);
                if ((pos += 10) > 180)
                    pos = 1;
                Thread.Sleep(100);
            }*/
        }
    }
}
