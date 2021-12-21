// yahboom
// https://github.com/lzty634158/Tiny-bit
// P0 buzzer
// P1 voice sensor/NC.... is there a jumper? J3?
// P12 neopixel x2
// P13 left lines sensor, digital
// P14 right line sensor, digital
// P15 ultrasonic echo
// P16 ultrasonic trig


// P19 I2C for PWM address 0x01. unknown chip!
// P20 I2C for PWM
// for RGB 0x01, red, green, blue
// for motor 0x02, left, left, right, right

using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.Signals;

namespace GHIElectronics.TinyCLR.Yahboom.TinyBit {
    class TinyBitController {
        
        private I2cDevice i2c;
        private AdcChannel voiceSensor;
        private GpioPin leftLineSensor, rightLineSensor, distanceTrigger, distanceEcho;
        private WS2812Controller ws2812;
        private byte[] b4 = new byte[4];
        private byte[] b5 = new byte[5];
        private PwmChannel buzzer;
        private PulseFeedback pulseFeedback;
        public void Beep() {
            this.buzzer.Controller.SetDesiredFrequency(4000);
            this.buzzer.SetActiveDutyCyclePercentage(0.5);
            this.buzzer.Start();
            Thread.Sleep(50);
            this.buzzer.Stop();
        }
        public void SetMotorSpeed(double left, double right) {
            this.b5[0] = 0x02;

            if (left > 0) {
                this.b5[1] = (byte)(left * 255);
                this.b5[2] = 0x00;
            }
            else {
                left *= -1;
                this.b5[1] = 0x00;
                this.b5[2] = (byte)(left * 255);
            }

            if (right > 0) {
                this.b5[3] = (byte)(right * 255);
                this.b5[4] = 0x00;
            }
            else {
                right *= -1;
                this.b5[3] = 0x00;
                this.b5[4] = (byte)(right * 255);
            }
            this.i2c.Write(this.b5);
        }
        public void SetHeadlight(int red, int green, int blue) {
            this.b4[0] = 0x01;
            this.b4[1] = (byte)(red);
            this.b4[2] = (byte)(green);
            this.b4[3] = (byte)(blue);
            this.i2c.Write(this.b4);
        }
        public TinyBitController(I2cController i2cController, PwmChannel buzzer, AdcChannel voiceSensor, GpioPin leftLineSensor, GpioPin rightLineSensor, GpioPin distanceTrigger, GpioPin distanceEcho, int colorLedPin) {
            this.i2c = i2cController.GetDevice(new I2cConnectionSettings(0x01,400_000));
            this.buzzer = buzzer;
            this.voiceSensor = voiceSensor;
            this.leftLineSensor = leftLineSensor;
            this.leftLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.rightLineSensor = rightLineSensor;
            this.rightLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            //var sg = new SignalGenerator(GpioController.GetDefault().OpenPin(colorLedPin));
            
            this.ws2812 = new WS2812Controller(GpioController.GetDefault().OpenPin( colorLedPin), 2, WS2812Controller.DataFormat.rgb888);
            this.distanceEcho = distanceEcho;
            this.distanceTrigger = distanceTrigger;

            this.pulseFeedback = new PulseFeedback(this.distanceTrigger, this.distanceEcho, PulseFeedbackMode.EchoDuration) {
                DisableInterrupts = false,
                Timeout = TimeSpan.FromSeconds(1),
                PulseLength = TimeSpan.FromTicks(100),
                PulseValue = GpioPinValue.High,
                EchoValue = GpioPinValue.High,
            };
        }
        public int ReadDistance() {
            var time = this.pulseFeedback.Trigger();
            var microsecond = time.TotalMilliseconds * 1000.0;

            var distance = microsecond * 0.036 / 2;

            return (int)distance;
        }
        public bool ReadLineSensor(bool left) {
            if (left)
                return this.leftLineSensor.Read() == GpioPinValue.High;
            else
                return this.rightLineSensor.Read() == GpioPinValue.High;
        }
        public double ReadVoiceLevel() => this.voiceSensor.ReadRatio();
        public void SetColorLeds(int index, int red, int green, int blue) {
            this.ws2812.SetColor(index, (byte)red, (byte)green, (byte)blue);
            this.ws2812.Flush();
        }
    }
}
