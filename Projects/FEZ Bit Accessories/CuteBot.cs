//elecfrek
// https://github.com/elecfreaks/pxt-cutebot/blob/master/cutebot.ts
// P8 ultrasonic trig
// P12 ultrasonic echo
// P13 left line sensor, digital
// P14 right line sensor, digital
// P15 neopixel x24 in code but I see only x2

// P19 I2C for PWM address 0x10. unknown chip!
// P20 I2C for PWM
// for headlight RGB 0x04 left 0x08 right, red, green, blue max255
// for motor 1=left 2 =right, 1=CW 2=CCW, speed max 100, 0
// servo control 0x05 left 0x06 right, angle,0,0


using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812;
using GHIElectronics.TinyCLR.Devices.Signals;

namespace GHIElectronics.TinyCLR.Elecfreaks.TinyBit {
    class CuteBotController {
        private I2cDevice i2c;
        private GpioPin leftLineSensor, rightLineSensor;
        private PwmChannel buzzer;
        private byte[] b4 = new byte[4];
        private WS2812Controller ws2812;

        public CuteBotController(I2cController i2cController, PwmChannel buzzer, GpioPin leftLineSensor, GpioPin rightLineSensor, GpioPin colorLedPin) {
            this.i2c = i2cController.GetDevice(new I2cConnectionSettings(0x10, 50_000));
            this.buzzer = buzzer;
            this.leftLineSensor = leftLineSensor;
            this.leftLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.rightLineSensor = rightLineSensor;
            this.rightLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.ws2812 = new WS2812Controller(colorLedPin, 2, WS2812Controller.DataFormat.rgb888);
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
        public void SetHeadlight(bool left, int red, int green, int blue) {
            if(left)
                this.b4[0] = 0x04;
            else
                this.b4[0] = 0x08;

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
        public void SetColorLeds(int index, int red, int green, int blue) {
            this.ws2812.SetColor(index, (byte) red, (byte)green, (byte)blue);
            this.ws2812.Flush();
        }
    }
}
