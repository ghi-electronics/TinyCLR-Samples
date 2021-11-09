//elecfrek
// https://github.com/elecfreaks/pxt-TPBot/blob/master/main.ts

// P13 left line sensor, digital no pull
// P14 right line sensor, digital no pull
// P15 echo
// P16 trig

// P19 I2C for PWM address 0x10. unknown chip!
// P20 I2C for PWM
// motor speed: 0x01, leftspeed, rightspeed, direction?(0,1,2,3)
// for headlight RGB 0x20,r,g,b (max255)
// servo control (four of them 0x10, 0x11, 0x12, x013), angle

using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812;


namespace GHIElectronics.TinyCLR.Elecfreaks.TinyBit {
    class TpBotController {
        private I2cDevice i2c;
        private GpioPin leftLineSensor, rightLineSensor;
        private PwmChannel buzzer;
        private byte[] b4 = new byte[4];
      
        public TpBotController(I2cController i2cController, PwmChannel buzzer, GpioPin leftLineSensor, GpioPin rightLineSensor) {
            this.i2c = i2cController.GetDevice(new I2cConnectionSettings(0x10, 50_000));
            this.buzzer = buzzer;
            this.leftLineSensor = leftLineSensor;
            this.leftLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.rightLineSensor = rightLineSensor;
            this.rightLineSensor.SetDriveMode(GpioPinDriveMode.Input);
           
        }
        public void SetMotorSpeed(double left, double right) {
            this.b4[0] = 0x01;
            this.b4[3] = 0x00;

            if (left > 0) {
                this.b4[1] = 2;
            }
            else {
                this.b4[1] = 1;
                left *= -1;
            }
            this.b4[2] = (byte)(left * 100);
            this.i2c.Write(this.b4);

            this.b4[0] = 0x02;
            if (right > 0) {
                this.b4[1] = 2;
            }
            else {
                this.b4[1] = 1;
                right *= -1;
            }
            this.b4[2] = (byte)(right * 100);
            this.i2c.Write(this.b4);
        }
        public void SetHeadlight(int red, int green, int blue) {
            this.b4[0] = 0x20;
            this.b4[1] = (byte)(red);
            this.b4[2] = (byte)(green);
            this.b4[3] = (byte)(blue);
            this.i2c.Write(this.b4);
        }
        public void Beep() {
            this.buzzer.Controller.SetDesiredFrequency(4000);
            this.buzzer.SetActiveDutyCyclePercentage(0.5);
            this.buzzer.Start();
            Thread.Sleep(50);
            this.buzzer.Stop();
        }
        public bool ReadLineSensor(bool left) {
            if (left)
                return this.leftLineSensor.Read() == GpioPinValue.High;
            else
                return this.rightLineSensor.Read() == GpioPinValue.High;
        }
        public void SetServo(int servo, int angle) {
            var servoCode = 0x10;
            switch (servo) {
                case 0:
                    servoCode = 0x10;
                    break;
                case 1:
                    servoCode = 0x11;
                    break;
                case 2:
                    servoCode = 0x12;
                    break;
                case 3:
                    servoCode = 0x13;
                    break;
            }
            this.b4[0] = (byte)(servoCode);
            this.b4[1] = (byte)angle;
            this.b4[2] = 0;
            this.b4[3] = 0;
            this.i2c.Write(this.b4);
        }
    }
}
