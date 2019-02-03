using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drawing;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;

namespace Display {
    public static class Program {
        public static void Main() {

            var spi = SpiController.FromName(G30.SpiBus.Spi2);
            var gpio = GpioController.GetDefault();
            var st7735 = new ST7735Controller(spi.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, G30.GpioPin.PB1)), gpio.OpenPin(G30.GpioPin.PB0), gpio.OpenPin(G80.GpioPin.PB12));
            st7735.SetDataAccessControl(true, true, false, false);
            st7735.Enable();

            var disp = DisplayController.FromProvider(st7735);
            disp.SetConfiguration(new SpiDisplayControllerSettings { Width = 160, Height = 100 });

            var bl = gpio.OpenPin(G80.GpioPin.PA7);
            bl.Write(GpioPinValue.High);
            bl.SetDriveMode(GpioPinDriveMode.Output);

            var hdc = GraphicsManager.RegisterDrawTarget(new DrawTarget(disp));
            var screen = Graphics.FromHdc(hdc);

            var rnd = new Random();
            var x = 10;
            var y = 10;
            var vx = rnd.Next(20) - 10;
            var vy = rnd.Next(20) - 10;
            var color = new Pen(Color.Green);
            var teal = new SolidBrush(Color.Teal);

            // This is a special built-in font for simple SPI displays
            var font = new Font("GHIMono8x5", 8);

            while (true) {
                x += vx;
                y += vy;

                if (x >= disp.ActiveConfiguration.Width || x < 0) vx *= -1;
                if (y >= disp.ActiveConfiguration.Height || y < 0) vy *= -1;

                screen.Clear(Color.Black);
                screen.DrawString("Hello World!", font, teal, 40, 10);
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
}
