// https://github.com/pimoroni/pxt-scrollbit/blob/master/scrollbit.ts
// I2C chip on address 0x74

using System;
using System.Threading;
using System.Collections;
using System.Text;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace GHIElectronics.TinyCLR.Pimoroni.ScrollBit {

    class ScrollBitController {
        private byte[] gamma= new byte [256] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2,
        2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5,
        6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 11, 11,
        11, 12, 12, 13, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18,
        19, 19, 20, 21, 21, 22, 22, 23, 23, 24, 25, 25, 26, 27, 27, 28,
        29, 29, 30, 31, 31, 32, 33, 34, 34, 35, 36, 37, 37, 38, 39, 40,
        40, 41, 42, 43, 44, 45, 46, 46, 47, 48, 49, 50, 51, 52, 53, 54,
        55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
        71, 72, 73, 74, 76, 77, 78, 79, 80, 81, 83, 84, 85, 86, 88, 89,
        90, 91, 93, 94, 95, 96, 98, 99, 100, 102, 103, 104, 106, 107, 109, 110,
        111, 113, 114, 116, 117, 119, 120, 121, 123, 124, 126, 128, 129, 131, 132, 134,
        135, 137, 138, 140, 142, 143, 145, 146, 148, 150, 151, 153, 155, 157, 158, 160,
        162, 163, 165, 167, 169, 170, 172, 174, 176, 178, 179, 181, 183, 185, 187, 189,
        191, 193, 194, 196, 198, 200, 202, 204, 206, 208, 210, 212, 214, 216, 218, 220,
        222, 224, 227, 229, 231, 233, 235, 237, 239, 241, 244, 246, 248, 250, 252, 255};

        private byte[] buf = new byte[144];
        private byte[] b2 = new byte[2];
        private int frame = 0;
        private I2cDevice i2c;
        public const int COLS = 17;
        public const int ROWS = 7;
        const int REG_BANK = 0xfd;
        const int BANK_CONFIG = 0x0b;
        const int REG_SHUTDOWN = 0x0a;
        const int REG_MODE = 0x00;
        const int REG_AUDIOSYNC = 0x06;
        const int REG_FRAME = 0x01;
        const int REG_COLOR = 0x24;

        public ScrollBitController(I2cController i2cController) {
            var settings = new I2cConnectionSettings(0x74, 100_000);
            this.i2c = i2cController.GetDevice(settings);

            this.WriteByte(REG_BANK, BANK_CONFIG);

            Thread.Sleep(1);
            this.WriteByte(REG_SHUTDOWN, 0);
            Thread.Sleep(1);

            this.WriteByte(REG_SHUTDOWN, 1);
            Thread.Sleep(1);


            this.WriteByte(REG_MODE, 0);
            this.WriteByte(REG_AUDIOSYNC, 0);

            var enable = new byte[17];// let enable = pins.createBuffer(17);
            for (var i = 0; i < 17; i++)
                enable[i] = 255;
            this.WriteByte(REG_BANK, 0);
            this.WriteBuffer(0, enable);
            this.WriteByte(REG_BANK, 1);
            this.WriteBuffer(0, enable);
        }
        private void WriteBuffer(int register, byte[] value) {
            var temp = new byte[value.Length + 1];
            temp[0] = (byte)register;
            for (var x = 0; x < value.Length; x++) {
                temp[x + 1] = value[x];
            }
            this.i2c.Write(temp);//pins.i2cWriteBuffer(addr, temp, false);
        }
        private void WriteByte(int register, int value) {

            this.b2[0] = (byte)register;
            this.b2[1] = (byte)value;
            this.i2c.Write(this.b2);// pins.i2cWriteBuffer(addr, temp, false);
        }
        private int PixelAddr(int col, int row) {
            row = 7 - (row + 1);
            if (col > 8) {
                col = col - 8;
                row = 6 - (row + 8);
            }
            else {
                col = 8 - col;
            }
            return (col * 16) + row;
        }
        private byte CorrectGamma(byte brightness) => this.gamma[brightness & 0xff];
        public void SetPixel(int col, int row, int brightness = 128) {
            if (col < 0 || row < 0 || col >= COLS || row >= ROWS)
                return;
            //if (UPSIDE_DOWN) {col = (COLS - 1) - col; row = (ROWS - 1) - row}
            this.buf[this.PixelAddr(col, row)] = (byte)brightness;
        }
        public void Show() {
            //let corrected_buf: Buffer = pins.createBuffer(144);
            var corrected_buf = new byte[144];
            for (var x = 0; x < this.buf.Length; x++) {
                corrected_buf[x] = this.CorrectGamma(this.buf[x]);
            }

            this.WriteByte(REG_BANK, this.frame);
            this.WriteBuffer(REG_COLOR, corrected_buf);
            this.WriteByte(REG_BANK, BANK_CONFIG);
            this.WriteByte(REG_FRAME, this.frame);

            this.frame = this.frame == 0 ? 1 : 0;
        }

    }


}
