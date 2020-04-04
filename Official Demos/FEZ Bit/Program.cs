using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;
using System.Drawing;


using GHIElectronics.TinyCLR.Yahboom.BitBot;

namespace FEZ_Bit {
    class Program {
        private static ST7735Controller st7735;
        private const int SCREEN_WIDTH = 160;
        private const int SCREEN_HEIGHT = 128;

        static void Blinker() {
            var led = GpioController.GetDefault().OpenPin(
                FEZBit.GpioPin.Led);
            led.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                led.Write(GpioPinValue.High);
                Thread.Sleep(100);
                led.Write(GpioPinValue.Low);
                Thread.Sleep(100);
            }
        }
        static BitBotController bot;
        static Graphics screen;
        static Font font12;
        static void InitBot() {
            var chip = new GHIElectronics.TinyCLR.Drivers.Nxp.PCA9685.PCA9685Controller(
                I2cController.FromName(FEZBit.I2cBus.Edge));
            var buzzerController = PwmController.FromName(FEZBit.PwmChannel.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.PwmChannel.Controller3.EdgeP0Channel);
            var frontsensorenable = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.EdgeP9);
            var frontsensorvaluecontroller = AdcController.FromName(FEZBit.AdcChannel.Controller1.Id);
            var frontvalue = frontsensorvaluecontroller.OpenChannel(FEZBit.AdcChannel.Controller1.EdgeP3);
            var lineDetectLeft = AdcController.FromName(FEZBit.AdcChannel.Controller1.Id).OpenChannel(FEZBit.AdcChannel.Controller1.EdgeP2);
            var lineDetectRight = AdcController.FromName(FEZBit.AdcChannel.Controller1.Id).OpenChannel(FEZBit.AdcChannel.Controller1.EdgeP1);
            var p2remove = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.EdgeP1);
            p2remove.SetDriveMode(GpioPinDriveMode.Input);

            var wifien = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.WiFiEn);
            wifien.SetDriveMode(GpioPinDriveMode.Output);
            wifien.Write(GpioPinValue.High);
            var wifireset = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.WiFiReset);
            wifireset.SetDriveMode(GpioPinDriveMode.Output);
            wifireset.Write(GpioPinValue.High);
            var wifics = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.WiFiCs);
            wifics.SetDriveMode(GpioPinDriveMode.Output);
            wifics.Write(GpioPinValue.High);

            bot = new BitBotController(
                chip, buzzerChannel,
                lineDetectLeft, lineDetectRight,
                FEZBit.GpioPin.EdgeP14, FEZBit.GpioPin.EdgeP15,
                frontsensorenable, frontvalue,
                FEZBit.GpioPin.EdgeP16);

            bot.SetHeadlight(0.9, 0.2, 0);
            bot.SetStatusLeds(false, true, false);
            bot.SetColorLeds(0, 0xff, 0, 0);
            bot.SetColorLeds(1, 0, 0xff, 0);
            bot.SetColorLeds(2, 0, 0, 0xff);
            bot.Beep();
        }
        static void InitDisplay() {
            // Display Get Ready ////////////////////////////////////
            var spi = SpiController.FromName(FEZBit.SpiBus.Display);
            var gpio = GpioController.GetDefault();

            st7735 = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings
                (SpiChipSelectType.Gpio, FEZBit.GpioPin.DisplayCs)), //CS pin.
                gpio.OpenPin(FEZBit.GpioPin.DisplayRs), //RS pin.
                gpio.OpenPin(FEZBit.GpioPin.DisplayReset) //RESET pin.
            );

            var backlight = gpio.OpenPin(FEZBit.GpioPin.Backlight);
            backlight.SetDriveMode(GpioPinDriveMode.Output);
            backlight.Write(GpioPinValue.High);

            st7735.SetDataAccessControl(true, true, false, false); //Rotate the screen.
            st7735.SetDrawWindow(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
            st7735.Enable();
            // Create flush event
            Graphics.OnFlushEvent += Graphics_OnFlushEvent; ;

            // Create bitmap buffer
            screen = Graphics.FromImage(new Bitmap(SCREEN_WIDTH, SCREEN_HEIGHT));

            font12 = Resource.GetFont(Resource.FontResources.droid_reg12);
            screen.Clear();
            screen.DrawRectangle(new Pen(new SolidBrush(Color.White)), 10, 10, SCREEN_WIDTH - 20, SCREEN_HEIGHT - 20);
            screen.DrawString("FEZ Bit", font12, new SolidBrush(Color.Red), 55, 30);
            screen.DrawString("SITCore", font12, new SolidBrush(Color.Green), 50, 50);
            screen.DrawString("GHI Electronics", font12, new SolidBrush(Color.Blue), 25, 70);
            screen.Flush();

        }
        static void Main() {
            new Thread(Blinker).Start();
            InitBot();
            InitDisplay();


            while (false) {
                bot.SetMotorSpeed(0.5, 0.5);
                Thread.Sleep(2000);
                bot.SetMotorSpeed(0.5, -0.5);
                Thread.Sleep(500);
                bot.SetMotorSpeed(0, 0);
                Thread.Sleep(500);
                var d = bot.ReadFrontSensor();
                bot.Beep();
            }
            
            // Buzzer ///////////////////
            var pwmController3 = PwmController.FromName(FEZBit.PwmChannel.Controller3.Id);
            var buzzer = pwmController3.OpenChannel(FEZBit.PwmChannel.Controller3.BuzzerChannel);
            pwmController3.SetDesiredFrequency(500);
            buzzer.SetActiveDutyCyclePercentage(0.5);
            buzzer.Start();
            for (var f = 500; f < 5_000; f += 300) {
                pwmController3.SetDesiredFrequency(f);
                Thread.Sleep(1);
            }
            buzzer.Stop();

            screen.DrawString("Press A", font12, new SolidBrush(Color.Teal), 50, 90);
            screen.Flush();
            // wait for A button //////////////////////
            var buttonA = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.ButtonA);
            buttonA.SetDriveMode(GpioPinDriveMode.InputPullUp);
            while (buttonA.Read() == GpioPinValue.High) {
                Thread.Sleep(10);
            }
     
            screen.Clear();

            screen.DrawString("TinyCLR OS", font12, new SolidBrush(Color.Teal), 40, 70);
            for (var i = 0; i < 128; i += 8) {
                screen.DrawLine(new Pen(Color.Yellow), i, 0, 0, 128 - i);
                bot.Beep();
                screen.Flush();
            }
            for (var i = 0; i < 128; i += 8) {
                screen.DrawLine(new Pen(Color.Purple), 160-i, 0, 160, 128 - i);
                bot.Beep();
                screen.Flush();
            }
            Thread.Sleep(-1);
        }
        private static void Graphics_OnFlushEvent(IntPtr hdc, byte[] data) => st7735.DrawBuffer(data);
    }
}
