// Made by Yahboom
// original driver https://github.com/lzty634158/yahboom_mbit_en/blob/master/main.ts
// P0 buzzer
// P1 Left line sensor analog
// P2 Right line sensor analog
// P3 front sensor analog value
// P9 front line sensor enable
// P14 ultrasonic trig
// P15 ultrasonic echo
// P16 neopixel x3
// P19 I2C for PWM PCA9685
// P20 I2C for PWM PCA9685

using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Signals;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Nxp.PCA9685;
using GHIElectronics.TinyCLR.Drivers.Neopixel.WS2812;


namespace GHIElectronics.TinyCLR.Yahboom.BitBot {
    class BitBotController {
        private PCA9685Controller pcaController;
        private PwmChannel buzzer;
        private GpioPin frontSensorEnable;
        private AdcChannel frontSensorValue;
        private PulseFeedback pulseFeedback;
        private AdcChannel leftLineSensor, rightLineSensor;
        private WS2812 ws2812;
        public void SetColorLeds(int index, int red, int green, int blue) {
            this.ws2812.SetColor(index, red, green , blue);
            this.ws2812.Draw();
        }
        public int ReadDistance() {
            var time = this.pulseFeedback.GeneratePulse();
            return (int) time.TotalMilliseconds;
        }
        public double ReadLineSensor(bool left) {
            if (left)
                return this.leftLineSensor.ReadRatio();
            else
                return this.rightLineSensor.ReadRatio();
        }
        public double ReadFrontSensor() {
            this.frontSensorEnable.Write(GpioPinValue.Low);
            var v = this.frontSensorValue.ReadRatio();
            this.frontSensorEnable.Write(GpioPinValue.High);
            return v;
        }
        public void SetStatusLeds(bool left, bool middle, bool right) {
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C8, 0, middle ? 0 : 4095);
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C7, 0, left ? 0 : 4095);
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C6, 0, right ? 0 : 4095);
        }
        public void SetServo(int num, int value) {
            ///// This function is not tested
            // 50hz: 20,000 us
            var us = (value * 1800 / 180 + 600); // 0.6 ~ 2.4
            var pwm = us * 4096 / 20000;

            this.pcaController.SetDutyCycle((PCA9685Controller.Channel)(num + 2), 0, pwm);
        }
        public void Beep() {
            this.buzzer.Controller.SetDesiredFrequency(4000);
            this.buzzer.SetActiveDutyCyclePercentage(0.5);
            this.buzzer.Start();
            Thread.Sleep(50);
            this.buzzer.Stop();
        }
        public BitBotController(PCA9685Controller pcaController,
            PwmChannel buzzer,
            AdcChannel leftLineSensor,
            AdcChannel rightLineSensor,
            int distanceTrigPin,
            int distanceEchoPin,
            GpioPin frontSensorEnable,
            AdcChannel frontSensorValue,
            int colorLedPin) {
            this.pcaController = pcaController;
            this.buzzer = buzzer;
            this.pcaController.SetFrequency(50);
            this.frontSensorEnable = frontSensorEnable;
            this.frontSensorValue = frontSensorValue;
            this.leftLineSensor = leftLineSensor;
            this.rightLineSensor = rightLineSensor;
            
            this.pulseFeedback = new PulseFeedback(distanceTrigPin, distanceEchoPin, PulseFeedbackMode.DurationUntilEcho) {
                DisableInterrupts = false,
                Timeout = TimeSpan.FromSeconds(1),
                PulseLength = TimeSpan.FromTicks(100),
                PulsePinValue = GpioPinValue.High,
                EchoPinValue = GpioPinValue.High,
                PulsePinDriveMode = GpioPinDriveMode.Output,
                EchoPinDriveMode = GpioPinDriveMode.Input
            };

            this.frontSensorEnable.SetDriveMode(GpioPinDriveMode.Output);
            this.frontSensorEnable.Write(GpioPinValue.High);

            this.ws2812 = new WS2812(colorLedPin, 3);
        }
        public void SetMotorSpeed(double left, double right) {

            if (left > 0) {
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C12, 0, (int)(left * 4095));
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C13, 0, 0);
            }
            else {
                left *= -1;
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C12, 0, 0);
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C13, 0, (int)(left * 4095));
            }

            if (right > 0) {
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C15, 0, (int)(right * 4095));
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C14, 0, 0);
            }
            else {
                right *= -1;
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C15, 0, 0);
                this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C14, 0, (int)(right * 4095));
            }

        }
        public void SetHeadlight(double red, double green, double blue) {
            if (red > 1.0 || green > 1.0 || blue > 1.0)
                throw new ArgumentException();
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C0, 0, (int)(red * 4095));
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C1, 0, (int)(green * 4095));
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C2, 0, (int)(blue * 4095));

             }
    }
}
