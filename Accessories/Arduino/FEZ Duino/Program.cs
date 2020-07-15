using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

using GHIElectronics.TinyCLR.Drivers.Shield;

namespace FEZ_Duino {
    class Program {
        static void Blinker() {
            var led = GpioController.GetDefault().OpenPin(
                SC20100.GpioPin.PB0);
            led.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led.Write(GpioPinValue.High);
                Thread.Sleep(100);
                led.Write(GpioPinValue.Low);
                Thread.Sleep(100);
            }
        }
        static void TestLcdKeypad() {
            var lcdkeypad = new LcdKeypadShield();
            lcdkeypad.Clear();
            lcdkeypad.Print("SITCore TinyCLR");
            lcdkeypad.SetCursor(1, 3);
            lcdkeypad.Print("FEZ Duino");

            while (true) {
                var k = lcdkeypad.ReadKeyPress();
                if (k != LcdKeypadShield.Keys.None) {
                    lcdkeypad.Clear();
                    lcdkeypad.CursorHome();
                    switch (k) {
                        case LcdKeypadShield.Keys.Select:
                            lcdkeypad.Print("Select");
                            break;
                        case LcdKeypadShield.Keys.Up:
                            lcdkeypad.Print("Up");
                            break;
                        case LcdKeypadShield.Keys.Down:
                            lcdkeypad.Print("Down");
                            break;
                        case LcdKeypadShield.Keys.Left:
                            lcdkeypad.Print("Left");
                            break;
                        case LcdKeypadShield.Keys.Right:
                            lcdkeypad.Print("Right");
                            break;
                    }
                }
                Thread.Sleep(50);
            }
        }
        static void TestMusicShield() {
            
            var midi = new MusicalInstrumentShield(SC20100.UartPort.Uart5);
            while (true) {
                midi.PlayBasicDemo();
                Thread.Sleep(500);
            }
            
        }
        //static void TestShieldBot() {
        //    var gpio = GpioController.GetDefault();
        //    var l1 = gpio.OpenPin(SC20100.GpioPin.PA0);
        //    var l2 = gpio.OpenPin(SC20100.GpioPin.PA0);
        //    var r1 = gpio.OpenPin(SC20100.GpioPin.PA0);
        //    var r2 = gpio.OpenPin(SC20100.GpioPin.PA0);

        //    var pwmcontrller = PwmController.FromName(SC20100.PwmChannel.Controller1.Id);


        //    var bot = new ShieldBot(l1,l2,r1,r2);
        //    bot.SetSpeed(0.5, 0.5);
        //    bot.SetSpeed(-0.5, 0.5);
        //    bot.SetSpeed(0.5, -0.5);
        //    bot.SetSpeed(0, 0);

        //}
        static void Main() {
            new Thread(Blinker).Start();

            EpectrumSample.Run();

            TestMusicShield();
            TestLcdKeypad();

            Thread.Sleep(-1);
        }
    }
}
