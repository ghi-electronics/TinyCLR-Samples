using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Glide
{
    class Display
    {
        public static DisplayController DisplayController { get; set; }

        public static void InitializeDisplay()
        {
            var backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);

            backlight.SetDriveMode(GpioPinDriveMode.Output);

            backlight.Write(GpioPinValue.High);

            DisplayController = GHIElectronics.TinyCLR.Devices.Display.DisplayController.GetDefault();

            var controllerSetting = new GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings
            {
                // 480x272
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
            };

            DisplayController.SetConfiguration(controllerSetting);
            DisplayController.Enable();
        }

        public static int Width => 480;
        public static int Height => 272;
    }
}
