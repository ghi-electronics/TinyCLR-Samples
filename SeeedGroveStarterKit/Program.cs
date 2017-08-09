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

            Relay relay = new Relay(FEZ.GpioPin.D6);
            while (true)
            {
                relay.TurnOn();
                Thread.Sleep(1000);
                relay.TurnOff();
                Thread.Sleep(1000);
            }//*/
            //==============================
            /*Servo
            ServoMotor servo = new ServoMotor(FEZ.PwmPin.Controller2.Id,FEZ.PwmPin.Controller2.D5);
            int pos = 90;
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
            LedSocket led = new LedSocket(FEZ.GpioPin.D3);
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
            Buzzer buzz = new Buzzer(FEZ.GpioPin.D4);
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
            RotaryAngleSensor rot = new RotaryAngleSensor(FEZ.AdcChannel.A0);
            ServoMotor servo = new ServoMotor(FEZ.PwmPin.Controller2.Id, FEZ.PwmPin.Controller2.D5);
            while (true)
            {
                double d = rot.GetAngle();
                System.Diagnostics.Debug.WriteLine("->" + d);
                servo.SetPosition(d / 100 * 180);
                Thread.Sleep(100);
            }//*/
            //==============================
            /* Sound Sensor
            SoundSensor sound = new SoundSensor(FEZ.AdcChannel.A2);
            ServoMotor servo = new ServoMotor(FEZ.PwmPin.Controller2.Id, FEZ.PwmPin.Controller2.D5);
            while (true)
            {
                double d = sound.ReadLevel();
                servo.SetPosition(d / 100 * 180);
                Debug.WriteLine("-> " + d);
                Thread.Sleep(30);
            }//*/
            //==============================
            /* Button
            Button btn = new Button(FEZ.GpioPin.D8);
            if (btn.IsPressed())
                Debug.WriteLine("Not using Events");
            btn.ButtonPressed += Btn_ButtonPressed;
            Thread.Sleep(-1);//*/
            //==============================
            /*
            TouchSensor touch = new TouchSensor(FEZ.GpioPin.D7);
              if (touch.IsTouched())
                    Debug.WriteLine("Not using Events");

            //touch.Touched += Btn_ButtonPressed;
            touch.Untouched += Touch_Untouched;
            Thread.Sleep(-1);//*/
            //==============================
            /* Temperature Sensor
            TemperatureSensor temp = new TemperatureSensor(FEZ.AdcChannel.A1);
            while(true)
            {
                Debug.WriteLine("-> " + temp.ReadTemperature());
                Thread.Sleep(1000);
            }//*/
            //==============================
            /* Light Sensor
            LightSensor light = new LightSensor(FEZ.AdcChannel.A3);
            while(true)
            {
                Debug.WriteLine("-> " + light.ReadLightLevel());
                Thread.Sleep(1000);
            }//*/
            //==============================

            //* LCD RGB Backlight
            LcdRgbBacklight lcd = new LcdRgbBacklight();
            lcd.Clear();
            lcd.SetBacklightRGB(100, 100, 0);
            lcd.BlinkBacklight(true);
            lcd.BlinkBacklight(false);
            lcd.Write("*** TinyCLR ***");
            Thread.Sleep(1000);
            lcd.SetCursor(0, 1);
            lcd.Write("Count:");
            int count = 0;
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

        private static void Touch_Untouched() {
            Debug.WriteLine("Untouched in event");
        }

        private static void Btn_ButtonPressed() {
            Debug.WriteLine("Button Pressed in event");
        }
    }
}
