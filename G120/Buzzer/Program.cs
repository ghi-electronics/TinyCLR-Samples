using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Pwm;

namespace Buzzer {
    class Program {
        static void Main() {
            // Buzzer is on PA15.
            // Use intellisense to determine what controller hosts PA15
            var buzzerController = PwmController.FromName(G120E.PwmChannel.Controller0.Id);
            var buzzer = buzzerController.OpenChannel(G120E.PwmChannel.Controller0.P3_19);

            var freq = 2000.0;
            var delta = 100.0;
            buzzerController.SetDesiredFrequency(freq);
            // we are playing a square wave to make sound. Set duty cycle to 50%.
            buzzer.SetActiveDutyCyclePercentage(0.5);
            buzzer.Start();
            while (true) {
                buzzerController.SetDesiredFrequency(freq);
                freq += delta;
                if (freq < 1000 || freq > 5000)
                    delta *= -1;
                Thread.Sleep(20);
            }
        }
    }
}
