using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace GHIElectronics.TinyCLR.Drivers.ST7920 {
    //This code is a C# port of http://nopaste.info/5fa51041a7.html (link is not working)
    // The code is very badly written and shoudl be changed when fully functional
    // Last quadrant is not updating
    // The code DID NOT WORK. To be investigated further.
    // For SPI mode
    // PSB to GND
    // E is clock
    // RW is data
    // RS is chip select
    // max clock is 600ns = 1.666Mhz? It is only working at under 1Mhz!
    public class ST7920Controller {
        SpiDevice spi;
        GpioPin chipSelect, reset;
        byte[][] screen = new byte[16][];

        //Initialize Display
        public ST7920Controller(GpioPin resetPin, GpioPin chipSelect, SpiController spiController) {
            //Initialize Pins
            this.chipSelect = chipSelect;
            this.reset = resetPin;
            this.chipSelect.SetDriveMode(GpioPinDriveMode.Output);
            this.chipSelect.Write(GpioPinValue.High);
            this.reset.SetDriveMode(GpioPinDriveMode.Output);
            this.reset.Write(GpioPinValue.High);
            //Initialize and configure our SPI Module
            var settings = new SpiConnectionSettings() {
                ChipSelectType = SpiChipSelectType.None,
                //ChipSelectLine = cs,
                Mode = SpiMode.Mode0,
                ClockFrequency = 600_000,
            };
            this.spi = spiController.GetDevice(settings);

           // SPI.Configuration _config = new SPI.Configuration(Cpu.Pin.GPIO_NONE, false, 100, 100, false, true, 500, SpiModule);
           // spi = new SPI(_config);

            //Reset the display
            this.reset.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            this.reset.Write(GpioPinValue.High);
            Thread.Sleep(100);

            //init the jagged array
            for (var i = 0; i < 16; i++) {
                this.screen[i] = new byte[64];
            }

            this.Basic_function_set(1, 0);
            Thread.Sleep(1);
            this.Basic_function_set(1, 0);
            Thread.Sleep(1);
            this.Basic_display_control(1, 0, 0);
            Thread.Sleep(1);
            this.Basic_display_clear();
            Thread.Sleep(1);
            this.Basic_entry_mode_set(1, 0);

            Thread.Sleep(1);
            this.Basic_display_control(1, 0, 0);

            Thread.Sleep(1);
            this.Basic_function_set(1, 1);
            Thread.Sleep(1);
            this.Extended_extended_function_set(1, 1, 1);

            //clear the local video ram
            for (var x = 0; x < 16; x++) {
                for (var y = 0; y < 64; y++) {
                    this.screen[x][y] = 0x00;
                }
            }
        }

        byte[] ba = new byte[1];
        private void ShiftOut(byte b) {
            this.ba[0] = b;
            this.spi.Write(this.ba);
        }

        private void Write_byte(int RS, int RW, byte data) {
            var b1 = (byte)(128 + 64 + 32 + 16 + 8 + RW * 4 + RS * 2);

            this.ShiftOut(b1);
            this.ShiftOut((byte)(data & 0xf0));
            this.ShiftOut((byte)(data << 4));
        }

        private void Write(int RS, int RW, int DB7, int DB6, int DB5, int DB4, int DB3, int DB2, int DB1, int DB0) {

            var b1 = (byte)(128 + 64 + 32 + 16 + 8 + RW * 4 + RS * 2);
            var b2 = (byte)(128 * DB7 + 64 * DB6 + 32 * DB5 + 16 * DB4);
            var b3 = (byte)(128 * DB3 + 64 * DB2 + 32 * DB1 + 16 * DB0);

            this.ShiftOut(b1);
            this.ShiftOut(b2);
            this.ShiftOut(b3);
        }

        private void Basic_function_set(int dl, int re) => this.Write(0, 0, 0, 0, 1, dl, 0, re, 0, 0);

        private void Basic_display_control(int d, int c, int b) => this.Write(0, 0, 0, 0, 0, 0, 1, d, c, b);

        private void Basic_display_clear() => this.Write(0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

        private void Basic_entry_mode_set(int id, int s) => this.Write(0, 0, 0, 0, 0, 0, 0, 1, id, s);

        private void Extended_extended_function_set(int dl, int re, int c) => this.Write(0, 0, 0, 0, 1, dl, 0, re, c, 0);

        private void Basic_write_ram_byte(byte data) => this.Write_byte(1, 0, data);

        byte[] buffer = new byte[6];
        byte[] gdram_buffer = new byte[3];
        public void Display_print() {
            this.chipSelect.Write(GpioPinValue.High);
            this.buffer[0] = this.buffer[3] = 0xFA;

            for (var y = 0; y < 64; y++) {
                for (var x = 0; x < 16; x += 2) {
                    var y_addr = y;
                    var x_addr = x;
                    if (y_addr >= 32) {
                        // somethign si wrong here so we are not going to use the bottom half for now!
                        return;
                        y_addr = y_addr - 32;
                        x_addr = x_addr + 8;
                    }

                    //Y pos
                    var address = (byte)(y_addr + 128);
                    this.gdram_buffer[0] = 0xF8;
                    this.gdram_buffer[1] = (byte)(address & 0xF0);
                    this.gdram_buffer[2] = (byte)(address << 4);
                    this.spi.Write(this.gdram_buffer);
                    //Thread.Sleep(1);

                    //X pos
                    address = (byte)((x_addr / 2) + 128);
                    this.gdram_buffer[1] = (byte)(address & 0xF0);
                    this.gdram_buffer[2] = (byte)(address << 4);
                    this.spi.Write(this.gdram_buffer);
                    //Thread.Sleep(1);

                    // actual pixel data
                    this.buffer[1] = (byte)(this.screen[x][y] & 0xf0);
                    this.buffer[2] = (byte)(this.screen[x][y] << 4);
                    this.buffer[4] = (byte)(this.screen[x + 1][y] & 0xf0);
                    this.buffer[5] = (byte)(this.screen[x + 1][y] << 4);

                    this.spi.Write(this.buffer);
                    //Thread.Sleep(50);
                }
            }
            this.chipSelect.Write(GpioPinValue.Low);
        }
        public void Pixel(int x, int y, bool on) {
            if (on)
                this.Enable_pixel(x, y);
            else
                this.Disable_pixel(x, y);
        }
        private void Enable_pixel(int x, int y) {
            switch (x % 8) {
                case 0: this.screen[x / 8][y] |= 128; break;
                case 1: this.screen[x / 8][y] |= 64; break;
                case 2: this.screen[x / 8][y] |= 32; break;
                case 3: this.screen[x / 8][y] |= 16; break;
                case 4: this.screen[x / 8][y] |= 8; break;
                case 5: this.screen[x / 8][y] |= 4; break;
                case 6: this.screen[x / 8][y] |= 2; break;
                case 7: this.screen[x / 8][y] |= 1; break;
            }
        }

        private void Disable_pixel(int x, int y) {
            switch (x % 8) {
                //Pixel & with 255...
                case 0: this.screen[x / 8][y] &= 127; break; //-128
                case 1: this.screen[x / 8][y] &= 191; break; //-64
                case 2: this.screen[x / 8][y] &= 223; break; //-32
                case 3: this.screen[x / 8][y] &= 239; break; //-16
                case 4: this.screen[x / 8][y] &= 247; break; //-8
                case 5: this.screen[x / 8][y] &= 251; break; //-4
                case 6: this.screen[x / 8][y] &= 253; break; //-2
                case 7: this.screen[x / 8][y] &= 254; break; //-1
            }
        }
    }
}
