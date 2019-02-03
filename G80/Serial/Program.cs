using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drawing;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using System.Text;

namespace Serial {
    public static class Program {
        static UartController serial;
        static Font font = new Font("GHIMono8x5", 8);
        static Graphics screen;
        static int cx = 0, cy = 0;
        public static void Main() {
            // Get the display ready
            var spi = SpiController.FromName(G80.SpiBus.Spi2);
            var gpio = GpioController.GetDefault();
            var st7735 = new ST7735Controller(spi.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, G80.GpioPin.PD10)), gpio.OpenPin(G80.GpioPin.PE10), gpio.OpenPin(G80.GpioPin.PE12));
            st7735.SetDataAccessControl(true, true, false, false);
            st7735.Enable();
            var disp = DisplayController.FromProvider(st7735);
            disp.SetConfiguration(new SpiDisplayControllerSettings { Width = 160, Height = 128 });
            var bl = gpio.OpenPin(G80.GpioPin.PC7);
            bl.Write(GpioPinValue.High);
            bl.SetDriveMode(GpioPinDriveMode.Output);
            var hdc = GraphicsManager.RegisterDrawTarget(new DrawTarget(disp));
            screen = Graphics.FromHdc(hdc);
            screen.Clear(Color.Black);

            var btn1 = gpio.OpenPin(G80.GpioPin.PE0);
            btn1.SetDriveMode(GpioPinDriveMode.InputPullUp);
            btn1.ValueChanged += Btn1_ValueChanged;

            // Serial port
            serial = UartController.FromName(G80.UartPort.Usart1);
            serial.SetActiveSettings(9600, 8, UartParity.None, UartStopBitCount.One, UartHandshake.None);
            serial.Enable();
            serial.DataReceived += Serial_DataReceived;

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Serial_DataReceived(UartController sender, DataReceivedEventArgs e) {
            var data = new byte[serial.BytesToRead];
            var count = serial.Read(data);
            for (var i = 0; i < count; i++) {
                var s = new string((char)data[i], 1);
                screen.DrawString(s, font, new SolidBrush(Color.Teal), cx, cy);
                cx += 6;
                if(cx > 150) {
                    cx = 0;
                    cy += 8;
                    if(cy>110) {
                        cx = 0;  cy = 0;
                        screen.Clear(Color.Black);
                    }
                }

            }
            screen.Flush();
        }
        static int count = 0;
        private static void Btn1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            if (e.Edge == GpioPinEdge.FallingEdge)
                serial.Write(Encoding.UTF8.GetBytes("Hi " + count++ + Environment.NewLine));
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
