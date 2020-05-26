using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.Silead.GSL1680;
using GHIElectronics.TinyCLR.Pins;

namespace Silead_Gsl1680_Display
{
    class Program
    {
        static void Main()
        {
            var gpioController = GpioController.GetDefault();

            // Turn backlight on
            var backlight = gpioController.OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            // Init display controller
            var displayController = DisplayController.GetDefault();

            displayController.SetConfiguration(new GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings {
                Width = 480,
                Height = 272,
                DataFormat = GHIElectronics.TinyCLR.Devices.Display.DisplayDataFormat.Rgb565,
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            });

            displayController.Enable();

            // Draw on screen
            var screen = Graphics.FromHdc(displayController.Hdc);

            screen.Clear();
            screen.FillEllipse(new SolidBrush(Color.FromArgb(100, 0xFF, 0, 0)), 0, 0, 100, 100);
            screen.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0xFF)), 0, 100, 100, 100);
            screen.DrawEllipse(new Pen(Color.Blue), 100, 0, 100, 100);
            screen.DrawRectangle(new Pen(Color.Red), 0, 0, 100, 100);
            screen.DrawLine(new Pen(Color.Green, 5), 250, 0, 220, 240);
            screen.Flush();

            // Init Touch
            var i2cController = I2cController.FromName(SC20260.I2cBus.I2c1);
            var silead = new GSL1680Controller(i2cController, GpioController.GetDefault().OpenPin(SC20260.GpioPin.PJ14));
            silead.CursorChanged += Silead_CusorChanged;

            Thread.Sleep(-1);
        }

        private static void Silead_CusorChanged(GSL1680Controller sender, TouchEventArgs e) => Debug.WriteLine("Cusor Pos: X = " + (e.X) + ", Y = " + (e.Y));
    }
}
