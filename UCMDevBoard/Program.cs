using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

namespace UCMDevBoard {
    public static class Program {
        private static void Blinker() {
            var gpioController = GpioController.GetDefault();
            var led = gpioController.OpenPin(UCMStandard.GpioPin.C);

            led.SetDriveMode(GpioPinDriveMode.Output);

            var state = false;

            while (true) {
                state = !state;

                led.Write(state ? GpioPinValue.High : GpioPinValue.Low);

                Thread.Sleep(100);
            }
        }

        public static void Main() {
            UCMStandard.SetModel(UCMModel.UC5550);

            new Thread(Blinker).Start();

            var frequency = 1000;
            var pwmController = PwmController.FromName(UCMStandard.PwmChannel.A.Id);
            var buzzer = pwmController.OpenChannel(UCMStandard.PwmChannel.A.Pin);

            pwmController.SetDesiredFrequency(frequency);

            buzzer.SetActiveDutyCyclePercentage(0.5);
            buzzer.Start();

            while (true) {
                pwmController.SetDesiredFrequency(frequency);

                if ((frequency += 100) > 5000)
                    frequency = 1000;

                Thread.Sleep(20);
            }
        }
    }
}
