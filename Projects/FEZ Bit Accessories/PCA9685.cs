using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace GHIElectronics.TinyCLR.Drivers.Nxp.PCA9685 {
    class PCA9685Controller {
        private enum Register {
            MODE1 = 0x00,
            MODE2 = 0x01,
            SUBADR1 = 0x02,
            SUBADR2 = 0x03,
            SUBADR3 = 0x04,
            ALLCALLADR = 0x05,
            LED0_ON_L = 0x06,
            LED0_ON_H = 0x07,
            LED0_OFF_L = 0x08,
            LED0_OFF_H = 0x09,
            ALLLED_ON_L = 0xFA,
            ALLLED_ON_H = 0xFB,
            ALLLED_OFF_L = 0xFC,
            ALLLED_OFF_H = 0xFD,
            PRESCALE = 0xFE,
        }
        public enum Channel {
            C0 = 0,
            C1 = 1,
            C2 = 2,
            C3 = 3,
            C4 = 4,
            C5 = 5,
            C6 = 6,
            C7 = 7,
            C8 = 8,
            C9 = 9,
            C10 = 10,
            C11 = 11,
            C12 = 12,
            C13 = 13,
            C14 = 14,
            C15 = 15,
        }
        private I2cDevice i2c;
        private byte[] b1 = new byte[1];
        private byte[] b2 = new byte[2];
        private byte[] b5 = new byte[5];
        public PCA9685Controller(I2cController i2cController) {
            var settings = new I2cConnectionSettings(0x41);
            this.i2c = i2cController.GetDevice(settings);

            this.WriteRegister(Register.MODE1, 0);
        }
        private void WriteRegister(Register reg, byte data) {
            this.b2[0] = (byte)reg;
            this.b2[1] = data;
            this.i2c.Write(this.b2);
        }
        private byte ReadRegister(Register reg) {
            this.b1[0] = (byte)reg;
            this.i2c.WriteRead(this.b1, this.b1);
            return this.b1[0];
        }
        public void SetFrequency(int herts) {
            if (herts < 24 || herts > 1526) {
                throw new ArgumentOutOfRangeException("herts must be in the range of 24 to 1526.");
            }
            var prescaler = 25000000;
            prescaler /= 4096;
            prescaler /= herts;
            prescaler -= 1;

            var oldmode = this.ReadRegister(Register.MODE1);
            var newmode = (oldmode & 0x7F) | 0x10;

            this.WriteRegister(Register.MODE1, (byte)newmode);//sleep
            Thread.Sleep(5);
            this.WriteRegister(Register.PRESCALE, (byte)prescaler);
            this.WriteRegister(Register.MODE1, (byte)oldmode);// wake

            Thread.Sleep(5);

            this.WriteRegister(Register.MODE1, (byte)(oldmode | 0xA1));
        }
        public void SetDutyCycle(Channel channel, int on, int off) {
            if (channel < 0 || (int)channel > 15)
                return;
            this.b5[0] = (byte)((int)Register.LED0_ON_L + 4 * (int)channel);
            this.b5[1] = (byte)(on & 0xFF);
            this.b5[2] = (byte)((on >> 8) & 0xFF);
            this.b5[3] = (byte)(off & 0xFF);
            this.b5[4] = (byte)((off >> 8) & 0xFF);

            this.i2c.Write(this.b5);
        }
}
}
