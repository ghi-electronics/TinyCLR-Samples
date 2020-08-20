using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.ShijiLighting.APA102C;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using System.IO;
using EcstaticLed.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Display;

namespace EcstaticLed {
    class Program {
        const int PanelsWide = 2;
        const int PanelsHigh = 3;
        const int LcdWidth = 480;
        const int LcdHeight = 272;

        static Font font;
        static Graphics screen;

        static GpioPin ldrButton;
        static GpioPin appButton;
        static GpioPin modButton;

        static byte intensity = 4;
        static bool pauseApp;

        static uint[] patternTable;

        static void Main() {
            Thread.Sleep(20);

            InitPatternTable();
            InitLcd();
            InitButtons();
            DoDraw();

            Thread.Sleep(Timeout.Infinite);
        }
        static byte[] NativeCookData(Bitmap bitmap, byte intensity) {
            var buffer = bitmap.GetBitmap();

            return NativeCookData(buffer, bitmap.Width, bitmap.Height, intensity);
        }

        static byte[] NativeCookData(byte[] buffer, int width, int height, byte intensity) {
            var rawData = new byte[width * height * 4];

            var alpha = (byte)(0b1110_0000 | intensity);

            Color.Convert(buffer, Color.RgbFormat.Rgb565, rawData, Color.RgbFormat.Rgb8888, alpha, true);

            var data = BitConverter.GetBytes(rawData, patternTable);

            return data;
        }

        static void InitPatternTable() {
            const int Width = 32;
            const int Height = 48;
            const int DeepColor = 4;
            const int PanelPixelPerRow = 16;

            int[] ledWallPanelOrder = { 5, 4, 2, 3, 1, 0 };

            patternTable = new uint[Width * Height * DeepColor];

            var idx = 0;

            for (var pixel = 0; pixel < patternTable.Length; pixel += PanelPixelPerRow * DeepColor) {
                var x = (pixel >> 2) % Width;
                var y = (pixel >> 2) / Width;

                var panelNumber = ledWallPanelOrder[(x >> 4) + (y >> 4) * PanelsWide];

                var ledNumber = panelNumber * 256 + (x % 16) + ((y % 16) << 4);

                ledNumber *= 4;

                for (var p = 0; p < 16 * 4; p++) {
                    patternTable[idx++] = (uint)(ledNumber + p);
                }
            }
        }

        static void InitLcd() {
            const int Backlight = SC20260.GpioPin.PA15;

            var backlight = GpioController.GetDefault().OpenPin(Backlight);

            backlight.SetDriveMode(GpioPinDriveMode.Output);

            backlight.Write(GpioPinValue.Low);

            Thread.Sleep(100);

            backlight.Write(GpioPinValue.High);

            var displayController = DisplayController.GetDefault();

            var controllerSetting = new GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings {
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

            displayController.SetConfiguration(controllerSetting);
            displayController.Enable();

            screen = Graphics.FromHdc(displayController.Hdc);
            font = Resources.GetFont(Resources.FontResources.NinaB);
        }

        static void InitButtons() {
            var gpioController = GpioController.GetDefault();

            ldrButton = gpioController.OpenPin(SC20260.GpioPin.PE3);
            appButton = gpioController.OpenPin(SC20260.GpioPin.PB7);
            modButton = gpioController.OpenPin(SC20260.GpioPin.PD7);

            ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            appButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            modButton.SetDriveMode(GpioPinDriveMode.InputPullUp);

            new Thread(ThreadProccessButtons).Start();
        }

        static void ThreadProccessButtons() {
            while (true) {
                if (ldrButton.Read() == GpioPinValue.Low) {
                    while (ldrButton.Read() == GpioPinValue.Low) ;

                    intensity = (byte)((intensity <= 0) ? 4 : (intensity - 1));

                }

                if (modButton.Read() == GpioPinValue.Low) {
                    while (modButton.Read() == GpioPinValue.Low) ;

                    intensity = (byte)((intensity >= 31) ? 4 : (intensity + 1));
                }

                if (appButton.Read() == GpioPinValue.Low) {
                    while (appButton.Read() == GpioPinValue.Low) ;

                    pauseApp = !pauseApp;
                }

                Thread.Sleep(1);
            }
        }

        static void DoDraw() {
            try {
                var sd = StorageController.FromName(SC20260.StorageController.SdCard);
                var drive = FileSystem.Mount(sd.Hdc);
                var spiController = SpiController.FromName(SC20260.SpiBus.Spi5);

                var ledController = new APA102CController(spiController, (PanelsWide * PanelsHigh) << 8);

                var stream = new FileStream(@"A:\output.avi", FileMode.Open);

                var aviDecoder = new AviDecoder(stream);

                var fpsCounter = 0;
                var oneSecondStart = DateTime.Now;

                var textColor = new SolidBrush(Color.White);

                aviDecoder.Run();

                var frameCounter = 0;
                var pauseCounter = 0;

                while (true) {
                    var t1 = DateTime.Now;

                    if (pauseApp == true && pauseCounter > 0) {
                        goto display_text;
                    }

                    pauseCounter = pauseApp ? (pauseCounter + 1) : 0;

                    try {
                        Bitmap bitmap;
                        bitmap = aviDecoder.GetBimap();

                        screen.Clear();

                        if (bitmap != null) {
                            var data = NativeCookData(bitmap, (byte)(0b1110_0000 | intensity));

                            ledController.SetBuffer(data, 0, data.Length);

                            for (var i = 0; i < 2; i++) // flush twice to fix pannel 5 mesy sometime
                                ledController.Flush();

                            screen.DrawImage(bitmap, (LcdWidth - bitmap.Width) >> 1, (LcdHeight - bitmap.Height) >> 1);
                        }
                    }
                    catch {

                    }

display_text:

                    screen.DrawString("Status: " + (frameCounter < 1 ? "Loading... " : (pauseApp ? "Paused" : "Playing")), font, textColor, 10, 10);
                    screen.DrawString("Intensity: " + intensity, font, textColor, 10, 30);

                    screen.Flush();

                    frameCounter++;

                    if (frameCounter % 10 == 0) {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    Thread.Sleep(1);

                    if (aviDecoder != null && aviDecoder.headerInfo != null && aviDecoder.headerInfo.TimeBetweenFrames != 0) {
                        var t2 = DateTime.Now - t1;

                        if (t2.TotalMilliseconds < aviDecoder.headerInfo.TimeBetweenFrames) {
                            Thread.Sleep(aviDecoder.headerInfo.TimeBetweenFrames - (int)t2.TotalMilliseconds);
                        }

                        fpsCounter++;

                        var oneSecondEnd = DateTime.Now - oneSecondStart;
                        if (oneSecondEnd.TotalMilliseconds >= 1000) {
                            Debug.WriteLine("Fps = " + fpsCounter);
                            fpsCounter = 0;
                            oneSecondStart = DateTime.Now;
                        }
                    }
                }
            }
            catch {
                screen.Clear();
                screen.DrawString("Loading file from sd card failed.", font, new SolidBrush(Color.Red), 10, 10);
                screen.DrawString("Check the card and Reset application.", font, new SolidBrush(Color.Red), 10, 30);

                screen.Flush();

            }
        }
    }
}


