// this shield is made by many manufactures. Even GHI used to offer them 10 years ago!
// Simply a standard character 2x16 display
// the buttons are all connected normally to one analog pin using resistor dividers
// the dividers can be a problem if the shield is designed only for 5V systems since
// the output voltage will be up to 5V instead of 3.3V. The fix is easy by wiring
// the top resistor to 3.3V. Or find one that is compatible with 3.3V like one GHI used to make!
// here is an example from DFRobot https://www.dfrobot.com/product-51.html?search=lcd%20keypad%20shield&description=true

using System;
using System.Collections;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Display.HD44780;

namespace GHIElectronics.TinyCLR.Drivers.Shield {
    class LcdKeypadShield {
        private DisplayHD44780 lcd;
        private AdcChannel a0;
        public enum Keys {
            Up,
            Down,
            Left,
            Right,
            Select,
            None
        }
        public LcdKeypadShield() {
            var gpio = GpioController.GetDefault();
            var d4 = new GpioPin[4]{
                gpio.OpenPin(SC20100.GpioPin.PA2),
                gpio.OpenPin(SC20100.GpioPin.PC7),
                gpio.OpenPin(SC20100.GpioPin.PC6),
                gpio.OpenPin(SC20100.GpioPin.PC4) };
            var e = gpio.OpenPin(SC20100.GpioPin.PA15);
            var rs = gpio.OpenPin(SC20100.GpioPin.PC5);
            this.lcd = new DisplayHD44780(d4, e, rs);

            this.a0 = AdcController.FromName(SC20100.AdcChannel.Controller1.Id)
                .OpenChannel(SC20100.AdcChannel.Controller1.PA4); 
            this.lcd.Clear();

        }
        public void Print(string str) => this.lcd.Print(str);
        public void Clear() => this.lcd.Clear();
        public void CursorHome() => this.lcd.CursorHome();
        public void SetCursor(int row, int col) => this.lcd.SetCursor(row, col);
        public Keys ReadKeyPress() {
            var d = this.a0.ReadRatio();
            // tweak these numbers to match your board
            // if 5V reference then some button swill not work
            // see notes on very top!
            if (d < 0.1)
                return Keys.Right;
            if (d < 0.2)
                return Keys.Up;
            if (d < 0.4)
                return Keys.Down;
            if (d < 0.6)
                return Keys.Left;
            if (d < 0.8)
                return Keys.Select;

            return Keys.None;
        }
    }
}
