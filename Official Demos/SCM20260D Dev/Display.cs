using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace Demos {
    static class Display {
        public const int Width = 480;
        public const int Height = 272;

        public static DisplayController DisplayController { get; set; }

        public static void InitializeDisplay() {
            var backlight = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PA15);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            DisplayController = DisplayController.GetDefault();

            var controllerSetting = new ParallelDisplayControllerSettings {
                Width = Width,
                Height = Height,
                DataFormat = DisplayDataFormat.Rgb565,
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
    }
}
