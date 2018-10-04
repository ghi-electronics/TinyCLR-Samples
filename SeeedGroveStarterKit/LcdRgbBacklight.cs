using GHIElectronics.TinyCLR.Devices.I2c;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;

namespace SeeedGroveStarterKit {
    public class LcdRgbBacklight {
        private I2cDevice DisplayDevice;
        private I2cDevice BacklightDevice;

        private byte _displayfunction;
        private byte _displaycontrol;
        private byte _displaymode;
        private byte _numlines, _currline;


        private byte LCD_CLEARDISPLAY = 0x01;
        private byte LCD_RETURNHOME = 0x02;
        private byte LCD_ENTRYMODESET = 0x04;
        private byte LCD_DISPLAYCONTROL = 0x08;
        private byte LCD_CURSORSHIFT = 0x10;
        private byte LCD_FUNCTIONSET = 0x20;
        private byte LCD_SETCGRAMADDR = 0x40;
        private byte LCD_SETDDRAMADDR = 0x80;

        private byte LCD_DISPLAYON = 0x04;
        private byte LCD_DISPLAYOFF = 0x00;
        private byte LCD_CURSORON = 0x02;
        private byte LCD_CURSOROFF = 0x00;
        private byte LCD_BLINKON = 0x01;
        private byte LCD_BLINKOFF = 0x00;
        private byte LCD_ENTRYRIGHT = 0x00;
        private byte LCD_ENTRYLEFT = 0x02;
        private byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        private byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        private byte REG_MODE1 = 0x00;
        private byte REG_MODE2 = 0x01;
        private byte REG_OUTPUT = 0x08;

        public LcdRgbBacklight() {
            var settings = new I2cConnectionSettings((0x7c >> 1)) {
                BusSpeed = I2cBusSpeed.FastMode
            };

            //string aqs = I2cDevice.GetDeviceSelector("I2C1");
            this.DisplayDevice = I2cDevice.FromId(FEZ.I2cBus.I2c1, settings);

            settings = new I2cConnectionSettings((0xc4 >> 1)) {
                BusSpeed = I2cBusSpeed.FastMode
            };

            this.BacklightDevice = I2cDevice.FromId(FEZ.I2cBus.I2c1, settings);

            ////////////////////////////////////
            // get the display going

            //byte cols = 6;
            byte lines = 2;
            byte dotsize = 0;


            if (lines > 1) {
                this._displayfunction |= 0x08;// LCD_2LINE;
            }
            this._numlines = lines;
            this._currline = 0;

            // for some 1 line displays you can select a 10 pixel high font
            if ((dotsize != 0) && (lines == 1)) {
                this._displayfunction |= 0x04;// LCD_5x10DOTS;
            }

            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40ms after power rises above 2.7V
            // before sending commands. Arduino can turn on way befer 4.5V so we'll wait 50
            //delayMicroseconds(50000);
            Thread.Sleep(50);

            // this is according to the hitachi HD44780 datasheet
            // page 45 figure 23

            // Send function set command sequence
            this.command((byte)(this.LCD_FUNCTIONSET | this._displayfunction));
            //delayMicroseconds(4500);  // wait more than 4.1ms
            Thread.Sleep(5);

            // second try
            this.command((byte)(this.LCD_FUNCTIONSET | this._displayfunction));
            //delayMicroseconds(150);
            Thread.Sleep(1);

            // third go
            this.command((byte)(this.LCD_FUNCTIONSET | this._displayfunction));


            // finally, set # lines, font size, etc.
            this.command((byte)(this.LCD_FUNCTIONSET | this._displayfunction));

            // turn the display on with no cursor or blinking default
            this._displaycontrol = (byte)(this.LCD_DISPLAYON | this.LCD_CURSOROFF | this.LCD_BLINKOFF);
            this.EnableDisplay(true);

            // clear it off
            this.Clear();

            // Initialize to default text direction (for romance languages)
            this._displaymode = (byte)(this.LCD_ENTRYLEFT | this.LCD_ENTRYSHIFTDECREMENT);
            // set the entry mode
            this.command((byte)(this.LCD_ENTRYMODESET | this._displaymode));


            // backlight init
            this.WriteBacklightReg(this.REG_MODE1, 0);
            // set LEDs controllable by both PWM and GRPPWM registers
            this.WriteBacklightReg(this.REG_OUTPUT, 0xFF);
            // set MODE2 values
            // 0010 0000 -> 0x20  (DMBLNK to 1, ie blinky mode)
            this.WriteBacklightReg(this.REG_MODE2, 0x20);

            //setColorWhite();
            this.SetBacklightRGB(255, 0, 100);

        }

        /*********** mid level commands, for sending data/cmds */
        //private byte LCD_ADDRESS = (0x7c >> 1);
        /*private void i2c_send_byte(byte dta)
        {
            //int written, read;
            byte[] wb = new byte[2];
            //byte[] rb = new byte[2];
            wb[0] = dta;
            DisplayDevice.Write(wb);
            //I2C.WriteRead(LCD_ADDRESS, wb, 0, 1, rb, 0, 0, out written, out read);

        }*/

        /*private void i2c_send_byteS(byte[] dta, byte len)
        {
            int written, read;
            //byte[] wb = new byte[2];
            byte[] rb = new byte[2];
            //wb[0] = dta;

            //I2C.WriteRead(LCD_ADDRESS, dta, 0, len, rb, 0, 0, out written, out read);


        }*/

        // send command
        private void command(byte value) {
            var dta = new byte[2] { 0x80, value };
            this.DisplayDevice.Write(dta);
            //i2c_send_byteS(dta, 2);
        }

        // send data
        private void write(byte value) {

            var dta = new byte[2] { 0x40, value };
            //i2c_send_byteS(dta, 2);
            this.DisplayDevice.Write(dta);
            //return 1; // assume sucess
        }

        /********** high level commands, for the user! */

        public void Write(string s) {

            for (var i = 0; i < s.Length; i++)
                this.write((byte)s[i]);
        }
        public void Clear() {
            this.command(this.LCD_CLEARDISPLAY);        // clear display, set cursor position to zero
            //delayMicroseconds(2000);          // this command takes a long time!
            Thread.Sleep(2);
        }

        public void GoHome() {
            this.command(this.LCD_RETURNHOME);        // set cursor position to zero
            //delayMicroseconds(2000);        // this command takes a long time!
            Thread.Sleep(2);
        }

        public void SetCursor(byte col, byte row) {

            col = (byte)(row == 0 ? (col | 0x80) : (col | 0xc0));
            this.command(col);
            //byte[] dta = new byte[2] { 0x80, col };
            //DisplayDevice.Write(dta);

            //i2c_send_byteS(dta, 2);

        }

        // Turn the display on/off (quickly)
        public void EnableDisplay(bool on) {
            if (on)
                this._displaycontrol |= this.LCD_DISPLAYON;
            else
                this._displaycontrol &= (byte)~this.LCD_DISPLAYON;

            this.command((byte)(this.LCD_DISPLAYCONTROL | this._displaycontrol));
        }

        // =============================================================================

        // Control the backlight LED blinking
        private void WriteBacklightReg(byte addr, byte data) {
            var wb = new byte[2];
            wb[0] = addr;
            wb[1] = data;

            this.BacklightDevice.Write(wb);

            //I2C.WriteRead((0xc4 >> 1), wb, 0, 2, rb, 0, 0, out written, out read);

        }

        public void BlinkBacklight(bool on) {
            // blink period in seconds = (<reg 7> + 1) / 24
            // on/off ratio = <reg 6> / 256
            if (on) {
                this.WriteBacklightReg(0x07, 0x17);  // blink every second
                this.WriteBacklightReg(0x06, 0x7f);  // half on, half off
            }
            else {
                this.WriteBacklightReg(0x07, 0x00);
                this.WriteBacklightReg(0x06, 0xff);
            }
        }
        public void SetBacklightRGB(byte r, byte g, byte b) {

            byte REG_RED = 0x04;    // pwm2
            byte REG_GREEN = 0x03;      // pwm1
            byte REG_BLUE = 0x02;      // pwm0

            this.WriteBacklightReg(REG_RED, r);
            this.WriteBacklightReg(REG_GREEN, g);
            this.WriteBacklightReg(REG_BLUE, b);
        }
    }
}
