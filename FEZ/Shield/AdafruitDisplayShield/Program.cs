using System.Drawing;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drawing;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;

namespace AdafruitDisplayShield {
    public sealed class DrawTarget : BufferDrawTargetRgb444 {
        private readonly DisplayController parent;

        public DrawTarget(DisplayController parent) : base(parent.ActiveConfiguration.Width, parent.ActiveConfiguration.Height) => this.parent = parent;

        public override void Flush() => this.parent.DrawBuffer(0, 0, this.Width, this.Height, this.buffer, 0);
    }

    public static class Program {
        public static void Main() {
            var spi1Controller = SpiController.FromName(FEZ.SpiBus.Spi1);
            var gpioController = GpioController.GetDefault();

            var st7735 = new ST7735Controller(spi1Controller.GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, FEZ.GpioPin.D10)), gpioController.OpenPin(FEZ.GpioPin.D8));
            st7735.SetDataAccessControl(false, false, false, false);
            st7735.SetDrawWindow(0, 40);

            var display = DisplayController.FromProvider(st7735);
            display.SetConfiguration(new SpiDisplayControllerSettings { Width = 128, Height = 80, DataFormat = DisplayDataFormat.Rgb444 });
            display.Enable();

            var hdc = GraphicsManager.RegisterDrawTarget(new DrawTarget(display));
            var screen = Graphics.FromHdc(hdc);

            var cnt = 0;
            var font = new Font("GHIMono8x5", 8);

            while (true) {
                screen.Clear(Color.Black);

                screen.DrawString(cnt++.ToString(), font, new SolidBrush(Color.Purple), 0, 40);
                screen.DrawEllipse(new Pen(Color.Blue), 0, 0, 10, 10);
                screen.DrawRectangle(new Pen(Color.Red), 10, 0, 10, 10);
                screen.DrawLine(new Pen(Color.Green, 1), 25, 0, 22, 24);
                screen.DrawString("The quick brown fox jumped over the lazy dogs.", font, new SolidBrush(Color.Teal), new RectangleF(40, 0, 60, 40));

                var idx = 0;
                screen.DrawLine(new Pen(Color.Black, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.White, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Gray, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Red, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Green, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Blue, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Yellow, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Purple, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;
                screen.DrawLine(new Pen(Color.Teal, 1), 60 + idx, 40, 60 + idx, 70); idx += 4;

                screen.Flush();
            }
        }
    }
}
