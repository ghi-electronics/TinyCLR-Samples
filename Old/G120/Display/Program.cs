using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Drawing;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Display {
    class Program {
        static void Main() {
            var backlight = GpioController.GetDefault().OpenPin(G120E.GpioPin.P3_17);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.Low);

            var displayController = DisplayController.GetDefault();

            // Enter the proper display configurations
            displayController.SetConfiguration(new ParallelDisplayControllerSettings {
                Width = 320,
                Height = 240,
                DataFormat = DisplayDataFormat.Rgb565,
                PixelClockRate = 15_000_000,
                PixelPolarity = false,
                DataEnablePolarity = true,
                DataEnableIsFixed = true,
                HorizontalFrontPorch = 51,
                HorizontalBackPorch = 27,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 16,
                VerticalBackPorch = 8,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });

            displayController.Enable();
            backlight.Write(GpioPinValue.High);

            // Some needed objects
            var screen = Graphics.FromHdc(displayController.Hdc);
            var greenPen = new Pen(Color.Green);
            var redPen = new Pen(Color.Red);
            var whitePen = new Pen(Color.White);
            var font = Resource.GetFont(Resource.FontResources.NinaB);
            var logo = Resource.GetBitmap(Resource.BitmapResources.GHI_Electronics_Logo);

            // Start Drawing (to memory)
            screen.Clear(Color.Black);
            screen.FillRectangle(whitePen.Brush, 0, 0,
                displayController.ActiveConfiguration.Width - 1,
                displayController.ActiveConfiguration.Height - 1);
            screen.DrawEllipse(greenPen, 200, 30, 80, 60);
            screen.DrawLine(redPen, 0, 0, 479, 271);
            screen.DrawString("Hello World!", font, greenPen.Brush, 10, 100);
            screen.DrawImage(logo, 50, 150);
            // Flush the memory to the display. This is a very fast operation.
            screen.Flush();

            //touch.TouchMove += Touch_TouchMove;

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
