using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace GHIElectronics.TinyCLR.Display.HD44780 {
    class DisplayHD44780 {
        private const byte DISP_ON = 0xC;    //Turn visible LCD on
        private const byte CLR_DISP = 1;      //Clear display
        private const byte CUR_HOME = 2;      //Move cursor home and clear screen memory
        private const byte SET_CURSOR = 0x80;   //SET_CURSOR + X : Sets cursor position to X
        private readonly GpioPin[] pinsD4;
        private readonly GpioPin pinE, pinRS;

        public DisplayHD44780(GpioPin[] pinsD4, GpioPin pinE, GpioPin pinRS) {
            this.pinsD4 = pinsD4;
            this.pinE = pinE;
            this.pinRS = pinRS;
            foreach (var pin in this.pinsD4) {
                pin.SetDriveMode(GpioPinDriveMode.Output);
                pin.Write(GpioPinValue.Low);
            }
            this.pinE.SetDriveMode(GpioPinDriveMode.Output);
            this.pinE.Write(GpioPinValue.Low);
            this.pinRS.SetDriveMode(GpioPinDriveMode.Output);
            this.pinRS.Write(GpioPinValue.Low);

            // 4 bit data communication
            Thread.Sleep(50);

            this.WriteNibble(0b0011);
            Thread.Sleep(50);
            this.WriteNibble(0b0011);
            Thread.Sleep(50);
            this.WriteNibble(0b0011);
            Thread.Sleep(50);
            this.WriteNibble(0b0010);
            this.SendCommand(DISP_ON);
            this.SendCommand(CLR_DISP);
        }

        private void WriteNibble(int c) {
            this.pinsD4[3].Write(((c & 0x8) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.pinsD4[2].Write(((c & 0x4) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.pinsD4[1].Write(((c & 0x2) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.pinsD4[0].Write(((c & 0x1) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.pinE.Write(GpioPinValue.High); this.pinE.Write(GpioPinValue.Low);

        }

        private void SendData(int c) {
            this.pinRS.Write(GpioPinValue.High); //set LCD to data mode
            this.WriteNibble(c >> 4);
            this.WriteNibble(c);
            Thread.Sleep(1);
        }

        private void SendCommand(int c) {
            this.pinRS.Write(GpioPinValue.Low);
            this.WriteNibble(c>>4);
            this.WriteNibble(c);
            Thread.Sleep(1);

        }

        public void Print(string str) {
            //foreach (var c in str)
            //  this.SendData(c);
            for (var i = 0; i < str.Length; i++)
                this.SendData(str[i]);
        }

        public void Clear() => this.SendCommand(CLR_DISP);

        public void CursorHome() => this.SendCommand(CUR_HOME);

        public void SetCursor(int row, int col) => this.SendCommand(SET_CURSOR | row << 6 | col);
    }
}

