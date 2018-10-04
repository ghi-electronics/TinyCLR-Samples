using System;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System.Diagnostics;

namespace SeeedGroveStarterKit {
    public class Program {
        public static void Main() {
            // Add a slash (/) before the (/ *) to uncomment a block
            //==============================
            /*Relay

            var relay = new Relay(FEZ.GpioPin.D6);
            while (true)
            {
                relay.TurnOn();
                Thread.Sleep(1000);
                relay.TurnOff();
                Thread.Sleep(1000);
            }//*/
            //==============================
            /*Servo
            var servo = new ServoMotor(FEZ.PwmChannel.Controller4.Id,FEZ.PwmChannel.Controller4.D5);
            var pos = 90;
            while (true)
            {
                servo.SetPosition(pos);
                pos += 10;
                if (pos > 180)
                    pos = 0;
                Thread.Sleep(500);
            }//*/
            //==============================
            /* LED Socket
            var led = new LedSocket(FEZ.GpioPin.D3);
            led.TurnOn();
            Thread.Sleep(5000);
            led.TurnOff();
            Thread.Sleep(5000);
            led.Blink();
            Thread.Sleep(5000);
            led.TurnOff();
            Thread.Sleep(5000);
            led.TurnOn();
            Thread.Sleep(5000);
            led.Blink(10);
            Thread.Sleep(-1);//*/
            //==============================
            /* Buzzer
            var buzz = new Buzzer(FEZ.GpioPin.D4);
            buzz.Beep();
            Thread.Sleep(5000);
            buzz.TurnOn();
            Thread.Sleep(1000);
            buzz.TurnOff();
            while(true)
            {
                buzz.Beep();
                Thread.Sleep(1000);
            }//*/
            //==============================
            /* Rotary Angle Sensor
            var rot = new RotaryAngleSensor(FEZ.AdcChannel.A0);
            var servo = new ServoMotor(FEZ.PwmChannel.Controller4.Id, FEZ.PwmChannel.Controller4.D5);
            while (true)
            {
                var d = rot.GetAngle();
                Debug.WriteLine("->" + d);
                servo.SetPosition(d / 100 * 180);
                Thread.Sleep(100);
            }//*/
            //==============================
            /* Sound Sensor
            var sound = new SoundSensor(FEZ.AdcChannel.A2);
            var servo = new ServoMotor(FEZ.PwmChannel.Controller4.Id, FEZ.PwmChannel.Controller4.D5);
            while (true)
            {
                var d = sound.ReadLevel();
                servo.SetPosition(d / 100 * 180);
                Debug.WriteLine("-> " + d);
                Thread.Sleep(30);
            }//*/
            //==============================
            /* Button
            var btn = new Button(FEZ.GpioPin.D8);
            btn.ButtonPressed += Btn_ButtonPressed;
            while (true) {
                if (btn.IsPressed())
                    Debug.WriteLine("Not using Events");
                Thread.Sleep(50);
            }//*/
            //==============================
            /*
            var touch = new TouchSensor(FEZ.GpioPin.D7);
            touch.Untouched += Touch_Untouched;
            //touch.Touched += Btn_ButtonPressed;
            while (true) {
                if (touch.IsTouched())
                    Debug.WriteLine("Not using Events");
                Thread.Sleep(50);
            }//*/
            //==============================
            /* Temperature Sensor
            var temp = new TemperatureSensor(FEZ.AdcChannel.A1);
            while(true)
            {
                Debug.WriteLine("-> " + temp.ReadTemperature());
                Thread.Sleep(1000);
            }//*/
            //==============================
            /* Light Sensor
            var light = new LightSensor(FEZ.AdcChannel.A3);
            while(true)
            {
                Debug.WriteLine("-> " + light.ReadLightLevel());
                Thread.Sleep(1000);
            }//*/
            //==============================

            //* LCD RGB Backlight
            var lcd = new LcdRgbBacklight();
            lcd.Clear();
            lcd.SetBacklightRGB(100, 100, 0);
            lcd.BlinkBacklight(true);
            lcd.BlinkBacklight(false);
            lcd.Write("*** TinyCLR ***");
            Thread.Sleep(1000);
            lcd.SetCursor(0, 1);
            lcd.Write("Count:");
            var count = 0;
            while (true) {
                lcd.SetCursor(7, 1);
                lcd.Write(count.ToString());
                count++;
                lcd.SetBacklightRGB(100, 100, (byte)count);
                Thread.Sleep(100);
            }//*/
            //==============================
            //
            Thread.Sleep(-1);
        }

        // ================================================
        // ================================================

        private static void Touch_Untouched() => Debug.WriteLine("Untouched in event");

        private static void Btn_ButtonPressed() => Debug.WriteLine("Button Pressed in event");
    }
}
