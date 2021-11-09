// dfrobot
// https://github.com/DFRobot/pxt-maqueen/blob/master/maqueen.ts

// P1 ultrasonic trig
// P2 ultrasonic echo
// P8 left led
// P13 left line sensor, digital
// P12 right led
// P14 right line sensor, digital
// P15 neopixel x4

// P19 I2C for PWM address 0x10. unknown chip!
// P20 I2C for PWM
// for motor: 0 left 2 right, 0 CW 1 CCW, speed 255 max
// for servo: 0x14 left 0x15 right, angle 0 to 180


using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812;
using GHIElectronics.TinyCLR.Devices.Signals;

namespace GHIElectronics.TinyCLR.Dfrobot.MicroMaqueen {
    class MaqueenController {
        private I2cDevice i2c;
        private GpioPin leftLineSensor, rightLineSensor;
        private GpioPin leftHeadlight, rightHeadlight;
        private PwmChannel buzzer;
        private byte[] b3 = new byte[3];
        private WS2812Controller ws2812;

        public MaqueenController(I2cController i2cController, PwmChannel buzzer, GpioPin leftHeadlight, GpioPin rightHeadlight, GpioPin leftLineSensor, GpioPin rightLineSensor, GpioPin colorLedPin) {
            this.i2c = i2cController.GetDevice(new I2cConnectionSettings(0x10, 100_000));
            this.buzzer = buzzer;
            this.leftLineSensor = leftLineSensor;
            this.leftLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.rightLineSensor = rightLineSensor;
            this.rightLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.leftHeadlight = leftHeadlight;
            this.leftHeadlight.SetDriveMode(GpioPinDriveMode.Output);
            this.rightHeadlight = rightHeadlight;
            this.rightHeadlight.SetDriveMode(GpioPinDriveMode.Output);

            this.ws2812 = new WS2812Controller(colorLedPin, 2, WS2812Controller.DataFormat.rgb888);
        }
        public void SetMotorSpeed(double left, double right) {
            this.b3[0] = 0x00;
            if (left > 0) {
                this.b3[1] = 0;
            }
            else {
                this.b3[1] = 1;
                left *= -1;
            }
            this.b3[2] = (byte)(left * 255);
            this.i2c.Write(this.b3);

            this.b3[0] = 0x02;
            if (right > 0) {
                this.b3[1] = 0;
            }
            else {
                this.b3[1] = 1;
                right *= -1;
            }
            this.b3[2] = (byte)(right * 255);
            this.i2c.Write(this.b3);
        }
        public void SetHeadlight(bool left, bool right) {
            if (left)
                this.leftHeadlight.Write(GpioPinValue.High);
            else
                this.leftHeadlight.Write(GpioPinValue.Low);
            if (right)
                this.rightHeadlight.Write(GpioPinValue.High);
            else
                this.rightHeadlight.Write(GpioPinValue.Low);
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
            this.ws2812.SetColor(index, (byte)red, (byte)green, (byte)blue);
            this.ws2812.Flush();
        }
    }
}
