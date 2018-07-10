using System;
using System.Diagnostics;
using System.Threading;
using Polulou.Zumo;

namespace PololuZumoArduino {
    public class Program {
        public static void Main() {
            bool state = false;


            while (!ZumoBot.ButtonIsPressed()) {
                state = !state;
                ZumoBot.SetLed(state);
                ZumoBot.Beep();
                Thread.Sleep(500);
                //Debug.WriteLine("X: " + ZumoBot.Accelerometer.ReadX() + " Y: " + ZumoBot.Accelerometer.ReadY());
                //Debug.WriteLine("X: " + ZumoBot.Gyroscope.ReadX() + " Y: " + ZumoBot.Gyroscope.ReadY());
            }
            Thread.Sleep(300);
            ZumoBot.SetLed(false);
            Thread.Sleep(300);
            ZumoBot.SetLed(true);
            Thread.Sleep(300);
            ZumoBot.SetLed(false);
            Thread.Sleep(300);
            ZumoBot.SetLed(true);

            while (true) {
                long left = ZumoBot.Reflectors.GetLevel(0);
                long right = ZumoBot.Reflectors.GetLevel(5);
                //Debug.WriteLine(">" + ZumoBot.Reflectors.GetLevel(0));
                //System.Diagnostics.Debug.WriteLine("l: " + left + " r: " + right);
                if (left < 8000 && right < 8000) {
                    ZumoBot.Motors.Move(40, 40);
                }
                else {
                    // backup and turn
                    ZumoBot.Motors.MoveBackward();
                    Thread.Sleep(500);
                    ZumoBot.Motors.Stop();
                    Thread.Sleep(1000);
                    ZumoBot.Motors.TurnLeft();
                    Thread.Sleep(500);
                    ZumoBot.Motors.Stop();
                    Thread.Sleep(1000);
                }
                Thread.Sleep(30);
            }
        }
    }
}
