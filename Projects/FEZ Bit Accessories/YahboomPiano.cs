// yahboom
// https://github.com/lzty634158/YB_Piano
// P0 speaker
// P1 neopixel x3

// P19 I2C for PWM address 0x50. AT42QT2160
// P20 I2C for PWM
// write 8, read 2 bytes with first byte is LSB

using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Neopixel.WS2812;

namespace GHIElectronics.TinyCLR.Yahboom.Piano {
    class YahboomPianoController {
        private I2cDevice i2c;
        private PwmChannel buzzer;
        private WS2812 ws2812;
        private byte[] b1 = new byte[1];
        private byte[] b2 = new byte[2];

        public YahboomPianoController(I2cController i2cController, PwmChannel buzzer, int colorLedPin) {
            this.i2c = i2cController.GetDevice(new I2cConnectionSettings(0x50, 100_000));
            this.buzzer = buzzer;
            this.ws2812 = new WS2812(GpioController.GetDefault().OpenPin(colorLedPin), 2);
        }
        public void Beep() {
            this.buzzer.Controller.SetDesiredFrequency(4000);
            this.buzzer.SetActiveDutyCyclePercentage(0.5);
            this.buzzer.Start();
            Thread.Sleep(50);
            this.buzzer.Stop();
        }
        public int ReadTouch() {
            this.b1[0] = 8;
            this.i2c.WriteRead(this.b1, this.b2);
            int r = this.b2[1];
            r <<= 8;
            r |= this.b2[0];
            return r;
        }
        public void SetColorLeds(int index, int red, int green, int blue) {
            this.ws2812.SetColor(index, red, green, blue);
            this.ws2812.Draw();
        }
    }
}
