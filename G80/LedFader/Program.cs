using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

namespace LedFader {
    class Program {
        static void Main() {
            // Use intellisense to determine what controller PE14 belongs to, controller1 in this example.
            var pwmCont = PwmController.FromName(G80.PwmChannel.Controller1.Id);
            var led1 = pwmCont.OpenChannel(G80.PwmChannel.Controller1.PE14);
            pwmCont.SetDesiredFrequency(1000);
            var level = 0.5;
            var vLevel = 0.1;
            led1.SetActiveDutyCyclePercentage(level);
            led1.Start();
            while (true) {
                level += vLevel;
                if (level < 0.1 || level > 0.9)
                    vLevel *= -1;
                led1.SetActiveDutyCyclePercentage(level);
                Thread.Sleep(20);
            }
        }
    }
}
