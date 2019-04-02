using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace UD435 {
    public static class Program {
        public static void Main() {
            UCMStandard.SetModel(UCMModel.UC5550);

            var gpioController = GpioController.GetDefault();
            var displayController = DisplayController.GetDefault();

            var backlight = gpioController.OpenPin(UCMStandard.GpioPin.A);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            displayController.SetConfiguration(new ParallelDisplayControllerSettings {
                Width = 480,
                Height = 272,
                DataFormat = DisplayDataFormat.Rgb565,
                HorizontalBackPorch = 46,
                HorizontalFrontPorch = 16,
                HorizontalSyncPolarity = false,
                HorizontalSyncPulseWidth = 1,
                DataEnableIsFixed = false,
                DataEnablePolarity = false,
                PixelClockRate = 18_000_000,
                PixelPolarity = false,
                VerticalBackPorch = 23,
                VerticalFrontPorch = 7,
                VerticalSyncPolarity = false,
                VerticalSyncPulseWidth = 1
            });

            displayController.Enable();

            var screen = Graphics.FromHdc(displayController.Hdc);
            var greenPen = new Pen(Color.Green, 5);

            int x = 50, y = 50, dx = 5, dy = 4;

            while (true) {
                screen.Clear(Color.Black);
                screen.DrawEllipse(greenPen, x, y, 10, 10);
                screen.Flush();

                x += dx;
                y += dy;

                if (x < 0 || x > displayController.ActiveConfiguration.Width) dx *= -1;
                if (y < 0 || y > displayController.ActiveConfiguration.Height) dy *= -1;

                Thread.Sleep(20);
            }
        }
    }
}
