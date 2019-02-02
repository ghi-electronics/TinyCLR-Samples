using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drawing;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Drawing;
using System.Threading;

namespace Display {
    public static class Program {
        public static void Main() {
            var spi = SpiController.FromName(G80.SpiBus.Spi2);
            var gpio = GpioController.GetDefault();
            var st7735 = new ST7735Controller(spi.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, G80.GpioPin.PD10)), gpio.OpenPin(G80.GpioPin.PE10), gpio.OpenPin(G80.GpioPin.PE12));

            var disp = DisplayController.FromProvider(st7735);
            disp.SetConfiguration(new SpiDisplayControllerSettings { Width = 160, Height = 128 });

            var bl = gpio.OpenPin(G80.GpioPin.PC7);
            bl.Write(GpioPinValue.High);
            bl.SetDriveMode(GpioPinDriveMode.Output);

            var hdc = GraphicsManager.RegisterDrawTarget(new DrawTarget(disp));
            var screen = Graphics.FromHdc(hdc);

            var rnd = new Random();
            var x = rnd.Next(160);
            var y = rnd.Next(128);
            var vx = rnd.Next(20) - 10;
            var vy = rnd.Next(20) - 10;
            var color = new Pen(Color.Green);

            while (true) {
                x += vx;
                y += vy;

                if (x >= 160 || x < 0) vx *= -1;
                if (y >= 128 || y < 0) vy *= -1;

                screen.Clear(Color.Black);
                screen.DrawEllipse(color, x, y, 10, 10);
                screen.Flush();
                Thread.Sleep(10);
            }
        }
    }

    public sealed class DrawTarget : IDrawTarget {
        private readonly DisplayController parent;
        private readonly byte[] buffer;

        public DrawTarget(DisplayController parent) {
            this.parent = parent;

            this.Width = parent.ActiveConfiguration.Width;
            this.Height = parent.ActiveConfiguration.Height;

            this.buffer = new byte[this.Width * this.Height * 2];
        }

        public int Width { get; }
        public int Height { get; }

        public void Dispose() { }
        public byte[] GetData() => this.buffer;
        public Color GetPixel(int x, int y) => throw new NotSupportedException();

        public void Clear(Color color) => Array.Clear(this.buffer, 0, this.buffer.Length);

        public void Flush() => this.parent.DrawBuffer(0, 0, this.Width, this.Height, this.buffer, 0);

        public void SetPixel(int x, int y, Color color) {
            if (x < 0 || y < 0 || x >= this.Width || y >= this.Height) return;

            var idx = (y * this.Width + x) * 2;
            var clr = color.ToArgb();
            var red = (clr & 0b0000_0000_1111_1111_0000_0000_0000_0000) >> 16;
            var green = (clr & 0b0000_0000_0000_0000_1111_1111_0000_0000) >> 8;
            var blue = (clr & 0b0000_0000_0000_0000_0000_0000_1111_1111) >> 0;

            this.buffer[idx] = (byte)((red & 0b1111_1000) | ((green & 0b1110_0000) >> 5));
            this.buffer[idx + 1] = (byte)(((green & 0b0001_1100) << 3) | ((blue & 0b1111_1000) >> 3));
        }
    }

    public class ST7735Controller : IDisplayControllerProvider {
        private readonly byte[] buffer1 = new byte[1];
        private readonly byte[] buffer4 = new byte[4];
        private readonly SpiDevice spi;
        private readonly GpioPin control;
        private readonly GpioPin reset;

        public static SpiConnectionSettings GetConnectionSettings(SpiChipSelectType chipSelectType, int chipSelectLine) => new SpiConnectionSettings {
            Mode = SpiMode.Mode3,
            ClockFrequency = 12_000_000,
            DataBitLength = 8,
            ChipSelectType = chipSelectType,
            ChipSelectLine = chipSelectLine
        };

        public ST7735Controller(SpiDevice spi, GpioPin control, GpioPin reset) {
            this.spi = spi;

            this.control = control;
            this.control.SetDriveMode(GpioPinDriveMode.Output);

            this.reset = reset;
            this.reset.SetDriveMode(GpioPinDriveMode.Output);

            this.reset.Write(GpioPinValue.Low);
            Thread.Sleep(50);

            this.reset.Write(GpioPinValue.High);
            Thread.Sleep(200);

            this.SendCommand(ST7735CommandId.SWRESET);
            Thread.Sleep(120);

            this.SendCommand(ST7735CommandId.SLPOUT);
            Thread.Sleep(120);

            this.SendCommand(ST7735CommandId.FRMCTR1);
            this.SendData(0x01);
            this.SendData(0x2C);
            this.SendData(0x2D);

            this.SendCommand(ST7735CommandId.FRMCTR2);
            this.SendData(0x01);
            this.SendData(0x2C);
            this.SendData(0x2D);

            this.SendCommand(ST7735CommandId.FRMCTR3);
            this.SendData(0x01);
            this.SendData(0x2C);
            this.SendData(0x2D);
            this.SendData(0x01);
            this.SendData(0x2C);
            this.SendData(0x2D);

            this.SendCommand(ST7735CommandId.INVCTR);
            this.SendData(0x07);

            this.SendCommand(ST7735CommandId.PWCTR1);
            this.SendData(0xA2);
            this.SendData(0x02);
            this.SendData(0x84);

            this.SendCommand(ST7735CommandId.PWCTR2);
            this.SendData(0xC5);

            this.SendCommand(ST7735CommandId.PWCTR3);
            this.SendData(0x0A);
            this.SendData(0x00);

            this.SendCommand(ST7735CommandId.PWCTR4);
            this.SendData(0x8A);
            this.SendData(0x2A);

            this.SendCommand(ST7735CommandId.PWCTR5);
            this.SendData(0x8A);
            this.SendData(0xEE);

            this.SendCommand(ST7735CommandId.VMCTR1);
            this.SendData(0x0E);

            this.SendCommand(ST7735CommandId.GAMCTRP1);
            this.SendData(0x0F);
            this.SendData(0x1A);
            this.SendData(0x0F);
            this.SendData(0x18);
            this.SendData(0x2F);
            this.SendData(0x28);
            this.SendData(0x20);
            this.SendData(0x22);
            this.SendData(0x1F);
            this.SendData(0x1B);
            this.SendData(0x23);
            this.SendData(0x37);
            this.SendData(0x00);
            this.SendData(0x07);
            this.SendData(0x02);
            this.SendData(0x10);

            this.SendCommand(ST7735CommandId.GAMCTRN1);
            this.SendData(0x0F);
            this.SendData(0x1B);
            this.SendData(0x0F);
            this.SendData(0x17);
            this.SendData(0x33);
            this.SendData(0x2C);
            this.SendData(0x29);
            this.SendData(0x2E);
            this.SendData(0x30);
            this.SendData(0x30);
            this.SendData(0x39);
            this.SendData(0x3F);
            this.SendData(0x00);
            this.SendData(0x07);
            this.SendData(0x03);
            this.SendData(0x10);

            this.SendCommand(ST7735CommandId.COLMOD);
            this.SendData(0x05);

            this.SendCommand(ST7735CommandId.MADCTL);
            this.SendData(0b1010_0000);

            this.buffer4[1] = 0;
            this.buffer4[3] = 159;
            this.SendCommand(ST7735CommandId.CASET);
            this.SendData(this.buffer4);

            this.buffer4[1] = 0;
            this.buffer4[3] = 127;
            this.SendCommand(ST7735CommandId.RASET);
            this.SendData(this.buffer4);

            this.SendCommand(ST7735CommandId.DISPON);
        }

        private void SendCommand(ST7735CommandId command) {
            this.buffer1[0] = (byte)command;
            this.control.Write(GpioPinValue.Low);
            this.spi.Write(this.buffer1);
        }

        private void SendData(byte data) {
            this.buffer1[0] = data;
            this.control.Write(GpioPinValue.High);
            this.spi.Write(this.buffer1);
        }

        private void SendData(byte[] data) {
            this.control.Write(GpioPinValue.High);
            this.spi.Write(data);
        }

        void IDisplayControllerProvider.DrawBuffer(int x, int y, int width, int height, byte[] data, int offset) {
            this.SendCommand(ST7735CommandId.RAMWR);
            this.control.Write(GpioPinValue.High);
            this.spi.Write(data, offset, data.Length);
        }

        DisplayInterface IDisplayControllerProvider.Interface => DisplayInterface.Spi;
        DisplayDataFormat[] IDisplayControllerProvider.SupportedDataFormats => new[] { DisplayDataFormat.Rgb565 };

        void IDisposable.Dispose() { }
        void IDisplayControllerProvider.Enable() { }
        void IDisplayControllerProvider.Disable() { }
        void IDisplayControllerProvider.SetConfiguration(DisplayControllerSettings configuration) { }
        void IDisplayControllerProvider.DrawString(string value) { }
        void IDisplayControllerProvider.DrawPixel(int x, int y, long color) { }
    }

    public enum ST7735CommandId : byte {
        //System
        NOP = 0x00,
        SWRESET = 0x01,
        RDDID = 0x04,
        RDDST = 0x09,
        RDDPM = 0x0A,
        RDDMADCTL = 0x0B,
        RDDCOLMOD = 0x0C,
        RDDIM = 0x0D,
        RDDSM = 0x0E,
        SLPIN = 0x10,
        SLPOUT = 0x11,
        PTLON = 0x12,
        NORON = 0x13,
        INVOFF = 0x20,
        INVON = 0x21,
        GAMSET = 0x26,
        DISPOFF = 0x28,
        DISPON = 0x29,
        CASET = 0x2A,
        RASET = 0x2B,
        RAMWR = 0x2C,
        RAMRD = 0x2E,
        PTLAR = 0x30,
        TEOFF = 0x34,
        TEON = 0x35,
        MADCTL = 0x36,
        IDMOFF = 0x38,
        IDMON = 0x39,
        COLMOD = 0x3A,
        RDID1 = 0xDA,
        RDID2 = 0xDB,
        RDID3 = 0xDC,

        //Panel
        FRMCTR1 = 0xB1,
        FRMCTR2 = 0xB2,
        FRMCTR3 = 0xB3,
        INVCTR = 0xB4,
        DISSET5 = 0xB6,
        PWCTR1 = 0xC0,
        PWCTR2 = 0xC1,
        PWCTR3 = 0xC2,
        PWCTR4 = 0xC3,
        PWCTR5 = 0xC4,
        VMCTR1 = 0xC5,
        VMOFCTR = 0xC7,
        WRID2 = 0xD1,
        WRID3 = 0xD2,
        NVCTR1 = 0xD9,
        NVCTR2 = 0xDE,
        NVCTR3 = 0xDF,
        GAMCTRP1 = 0xE0,
        GAMCTRN1 = 0xE1,
    }
}
