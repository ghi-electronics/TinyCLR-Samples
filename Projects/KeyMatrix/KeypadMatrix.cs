using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
//using GHIElectronics.TinyCLR.Pins;

namespace KeyMatrix {
    class KeypadMatrix {
        public delegate void KeypadEventHandler(char c);
        public event KeypadEventHandler KeyPressed;

        GpioPin row1;
        GpioPin row2;
        GpioPin row3;
        GpioPin row4;
        GpioPin col1;
        GpioPin col2;
        GpioPin col3;
        GpioPin col4;
        private char lastKey = (char)0;
        private void InvokeOnce(char c) {
            if(this.lastKey != c) {
                this.lastKey = c;
                this.KeyPressed?.Invoke(c);
            }
        }
        private void Scanner() {
            while (true) {

                // First Row
                this.row1.Write(GpioPinValue.Low); this.row2.Write(GpioPinValue.High); this.row3.Write(GpioPinValue.High); this.row4.Write(GpioPinValue.High);
                if (this.col1.Read() == GpioPinValue.Low)
                    this.InvokeOnce('1');
                if (this.col2.Read() == GpioPinValue.Low)
                    this.InvokeOnce('2');
                if (this.col3.Read() == GpioPinValue.Low)
                    this.InvokeOnce('3');
                if (this.col4.Read() == GpioPinValue.Low)
                    this.InvokeOnce('A');
                // Second Row
                this.row1.Write(GpioPinValue.High); this.row2.Write(GpioPinValue.Low); this.row3.Write(GpioPinValue.High); this.row4.Write(GpioPinValue.High);
                if (this.col1.Read() == GpioPinValue.Low)
                    this.InvokeOnce('4');
                if (this.col2.Read() == GpioPinValue.Low)
                    this.InvokeOnce('5');
                if (this.col3.Read() == GpioPinValue.Low)
                    this.InvokeOnce('6');
                if (this.col4.Read() == GpioPinValue.Low)
                    this.InvokeOnce('B');
                // Third Row
                this.row1.Write(GpioPinValue.High); this.row2.Write(GpioPinValue.High); this.row3.Write(GpioPinValue.Low); this.row4.Write(GpioPinValue.High);
                if (this.col1.Read() == GpioPinValue.Low)
                    this.InvokeOnce('7');
                if (this.col2.Read() == GpioPinValue.Low)
                    this.InvokeOnce('8');
                if (this.col3.Read() == GpioPinValue.Low)
                    this.InvokeOnce('9');
                if (this.col4.Read() == GpioPinValue.Low)
                    this.InvokeOnce('C');
                // Forth Row
                this.row1.Write(GpioPinValue.High); this.row2.Write(GpioPinValue.High); this.row3.Write(GpioPinValue.High); this.row4.Write(GpioPinValue.Low);
                if (this.col1.Read() == GpioPinValue.Low)
                    this.InvokeOnce('*');
                if (this.col2.Read() == GpioPinValue.Low)
                    this.InvokeOnce('0');
                if (this.col3.Read() == GpioPinValue.Low)
                    this.InvokeOnce('#');
                if (this.col4.Read() == GpioPinValue.Low)
                    this.InvokeOnce('D');

                Thread.Sleep(20);
            }
        }
        public KeypadMatrix(GpioPin[] pins) {

            this.row1 = pins[0];
            this.row2 = pins[1];
            this.row3 = pins[2];
            this.row4 = pins[3];

            this.col1 = pins[4];
            this.col2 = pins[5];
            this.col3 = pins[6];
            this.col4 = pins[7];

            this.row1.SetDriveMode(GpioPinDriveMode.Output);
            this.row2.SetDriveMode(GpioPinDriveMode.Output);
            this.row3.SetDriveMode(GpioPinDriveMode.Output);
            this.row4.SetDriveMode(GpioPinDriveMode.Output);

            this.col1.SetDriveMode(GpioPinDriveMode.InputPullUp);
            this.col2.SetDriveMode(GpioPinDriveMode.InputPullUp);
            this.col3.SetDriveMode(GpioPinDriveMode.InputPullUp);
            this.col4.SetDriveMode(GpioPinDriveMode.InputPullUp);

            new Thread(this.Scanner).Start();
        }  
    }
}

