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
// P19 I2C for PWM PCA9685... address 0x41
// P20 I2C for PWM PCA9685


// ON PWM Chip channels
// Ch12 Ch13 left motor
// Ch14 Ch15 right motor

using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Signals;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Nxp.PCA9685;
using GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812;


namespace GHIElectronics.TinyCLR.Yahboom.BitBot {
    class BitBotController {
        private PCA9685Controller pcaController;
        private PwmChannel buzzer;
        private GpioPin frontSensorEnable;
        private AdcChannel frontSensorValue;
        private PulseFeedback pulseFeedback;
        private AdcChannel leftLineSensor, rightLineSensor;
        private WS2812Controller ws2812;
        public void SetColorLeds(int index, int red, int green, int blue) {
            this.ws2812.SetColor(index, (byte)red, (byte)green, (byte)blue);
            this.ws2812.Flush();
        }
        public int ReadDistance() {
            var time = this.pulseFeedback.Trigger();
            var microsecond = time.TotalMilliseconds * 1000.0;

            var distance = microsecond * 0.036 / 2;

            return (int)distance;
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
            GpioPin distanceTrigPin,
            GpioPin distanceEchoPin,
            GpioPin frontSensorEnable,
            AdcChannel frontSensorValue,
            GpioPin colorLedPin) {
            this.pcaController = pcaController;
            this.buzzer = buzzer;
            this.pcaController.SetFrequency(50);
            this.frontSensorEnable = frontSensorEnable;
            this.frontSensorValue = frontSensorValue;
            this.leftLineSensor = leftLineSensor;
            this.rightLineSensor = rightLineSensor;

            this.pulseFeedback = new PulseFeedback(distanceTrigPin, distanceEchoPin, PulseFeedbackMode.EchoDuration) {
                DisableInterrupts = false,
                Timeout = TimeSpan.FromSeconds(1),
                PulseLength = TimeSpan.FromTicks(100),
                PulseValue = GpioPinValue.High,
                EchoValue = GpioPinValue.High,
            };

            this.frontSensorEnable.SetDriveMode(GpioPinDriveMode.Output);
            this.frontSensorEnable.Write(GpioPinValue.High);

            this.ws2812 = new WS2812Controller(colorLedPin, 3, WS2812Controller.DataFormat.rgb888);
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
        public void SetHeadlight(int red, int green, int blue) {
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C0, 0, (int)(red * 16));
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C1, 0, (int)(green * 16));
            this.pcaController.SetDutyCycle(PCA9685Controller.Channel.C2, 0, (int)(blue * 16));

        }
    }
}
