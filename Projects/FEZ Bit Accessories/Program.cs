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
using GHIElectronics.TinyCLR.Devices.Signals;
using GHIElectronics.TinyCLR.Yahboom.TinyBit;
using GHIElectronics.TinyCLR.Elecfreaks.TinyBit;
using GHIElectronics.TinyCLR.Waveshare.Display18;

namespace FEZ_Bit
{
    class Program
    {
        private static ST7735Controller st7735;
        private const int SCREEN_WIDTH = 160;
        private const int SCREEN_HEIGHT = 128;

        static void Blinker()
        {
            var led = GpioController.GetDefault().OpenPin(
                FEZBit.GpioPin.Led);
            led.SetDriveMode(GpioPinDriveMode.Output);
            while (true)
            {
                led.Write(GpioPinValue.High);
                Thread.Sleep(100);
                led.Write(GpioPinValue.Low);
                Thread.Sleep(100);
            }
        }
        static BitBotController bot;
        static Graphics screen;
        static Font font12;
        static void InitBot()
        {
            var chip = new GHIElectronics.TinyCLR.Drivers.Nxp.PCA9685.PCA9685Controller(
                I2cController.FromName(FEZBit.I2cBus.Edge));

            var gpioController = GpioController.GetDefault();
            var buzzerController = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.Timer.Pwm.Controller3.P0);
            var frontsensorenable = gpioController.OpenPin(FEZBit.GpioPin.P9);
            var frontsensorvaluecontroller = AdcController.FromName(FEZBit.Adc.Controller1.Id);
            var frontvalue = frontsensorvaluecontroller.OpenChannel(FEZBit.Adc.Controller1.P3);
            var lineDetectLeft = AdcController.FromName(FEZBit.Adc.Controller1.Id).OpenChannel(FEZBit.Adc.Controller1.P2);
            var lineDetectRight = AdcController.FromName(FEZBit.Adc.Controller1.Id).OpenChannel(FEZBit.Adc.Controller1.P1);
            var p2remove = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P1);
            p2remove.SetDriveMode(GpioPinDriveMode.Input);


            bot = new BitBotController(
                chip, buzzerChannel,
                lineDetectLeft, lineDetectRight,
                gpioController.OpenPin(FEZBit.GpioPin.P14), gpioController.OpenPin(FEZBit.GpioPin.P15),
                frontsensorenable, frontvalue,
                gpioController.OpenPin(FEZBit.GpioPin.P16));

            bot.SetHeadlight(200, 50, 0);
            bot.SetStatusLeds(false, true, false);
            bot.SetColorLeds(0, 0xff, 0, 0);
            bot.SetColorLeds(1, 0, 0xff, 0);
            bot.SetColorLeds(2, 0, 0, 0xff);
            bot.Beep();
        }
        static void InitDisplay()
        {
            // Display Get Ready ////////////////////////////////////
            var spi = SpiController.FromName(FEZBit.SpiBus.Display);
            var gpio = GpioController.GetDefault();

            st7735 = new ST7735Controller(
                spi.GetDevice(ST7735Controller.GetConnectionSettings
                (SpiChipSelectType.Gpio, GpioController.GetDefault().OpenPin(FEZBit.GpioPin.DisplayChipselect))), //CS pin.
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
            Graphics.OnFlushEvent += Graphics_OnFlushEvent;

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



        static void TestCuteBot()
        {
            var buzzerController = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.Timer.Pwm.Controller3.P0);
            var lineDetectLeft = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P13);
            var lineDetectRight = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P14);

            var bot = new GHIElectronics.TinyCLR.Elecfreaks.TinyBit.CuteBotController(
                I2cController.FromName(FEZBit.I2cBus.Edge),
                buzzerChannel,
                lineDetectLeft, lineDetectRight,
                GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P15)
                );

            bot.Beep();
            bot.SetColorLeds(1, 100, 0, 0);
            bot.SetColorLeds(0, 0, 50, 100);
            bot.SetHeadlight(true, 30, 100, 100);
            bot.SetHeadlight(false, 30, 0, 200);
            bot.SetMotorSpeed(0.5, 0.5);
            bot.SetMotorSpeed(0.5, -0.5);
            bot.SetMotorSpeed(-0.5, 0.5);
            bot.SetMotorSpeed(0, 0);
            while (true)
            {
                var l = bot.ReadLineSensor(true);
                var r = bot.ReadLineSensor(false);

                Thread.Sleep(50);
                bot.Beep();
            }

        }
        static void TestTpBot()
        {
            var buzzerController = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.Timer.Pwm.Controller3.P0);
            var lineDetectLeft = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P13);
            var lineDetectRight = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P14);
            var voiceSensor = AdcController.FromName(FEZBit.Adc.Controller1.Id).OpenChannel(FEZBit.Adc.Controller1.P1);
            var p2remove = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P1);
            var distanceTrigger = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P16);
            var distanceEcho = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P15);
            p2remove.SetDriveMode(GpioPinDriveMode.Input);

            var bot = new TpBotController(
                I2cController.FromName(FEZBit.I2cBus.Edge),
                buzzerChannel,
                lineDetectLeft, lineDetectRight);//, distanceTrigger, distanceEcho);


            new Thread(() => {
                while (true)
                {
                    bot.SetHeadlight(100, 0, 0);
                    Thread.Sleep(200);
                    bot.SetHeadlight(0, 0, 100);
                    Thread.Sleep(300);
                }
            }).Start();
            /*new Thread(() => {
                while (true) {
                    bot.Beep();
                    Thread.Sleep(2_000);
                }
            }).Start();
            */
            while (true)
            {
                bot.SetMotorSpeed(0.9, 0.9);
                var l = bot.ReadLineSensor(true);
                var r = bot.ReadLineSensor(false);
                //var v = bot.ReadVoiceLevel();
                /*var d = bot.ReadDistance();
                if (d < 20) {
                    bot.SetMotorSpeed(-0.9, -0.9);
                    Thread.Sleep(200);
                    bot.SetMotorSpeed(-0.9, 0.9);
                    Thread.Sleep(200);
                    bot.SetMotorSpeed(0, 0);
                    Thread.Sleep(1000);


                    bot.Beep();
                    Thread.Sleep(100);
                    bot.Beep();
                    Thread.Sleep(100);
                    bot.Beep();
                    Thread.Sleep(100);
                }*/
                Thread.Sleep(10);
            }
        }
        static void TestTinyBit()
        {
            var buzzerController = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.Timer.Pwm.Controller3.P0);
            var lineDetectLeft = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P13);
            var lineDetectRight = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P14);
            var voiceSensor = AdcController.FromName(FEZBit.Adc.Controller1.Id).OpenChannel(FEZBit.Adc.Controller1.P1);
            var p2remove = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P1);
            var distanceTrigger = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P16);
            var distanceEcho = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P15);
            p2remove.SetDriveMode(GpioPinDriveMode.Input);

            var bot = new TinyBitController(
                I2cController.FromName(FEZBit.I2cBus.Edge),
                buzzerChannel,
                voiceSensor,
                lineDetectLeft, lineDetectRight, distanceTrigger, distanceEcho,
                FEZBit.GpioPin.P12
                );


            new Thread(() => {
                while (true)
                {
                    bot.SetHeadlight(100, 0, 0);
                    Thread.Sleep(200);
                    bot.SetHeadlight(0, 0, 100);
                    Thread.Sleep(300);
                }
            }).Start();
            /*new Thread(() => {
                while (true) {
                    bot.Beep();
                    Thread.Sleep(2_000);
                }
            }).Start();
            */
            while (true)
            {
                bot.SetMotorSpeed(0.5, 0.5);
                var l = bot.ReadLineSensor(true);
                var r = bot.ReadLineSensor(false);
                var v = bot.ReadVoiceLevel();
                var d = bot.ReadDistance();
                if (d < 20)
                {
                    bot.SetMotorSpeed(-0.5, -0.5);
                    Thread.Sleep(200);
                    bot.SetMotorSpeed(-0.5, 0.5);
                    Thread.Sleep(200);
                    bot.SetMotorSpeed(0, 0);
                    Thread.Sleep(1000);


                    bot.Beep();
                    Thread.Sleep(100);
                    bot.Beep();
                    Thread.Sleep(100);
                    bot.Beep();
                    Thread.Sleep(100);
                }
                Thread.Sleep(10);
            }

            bot.SetHeadlight(30, 100, 100);
            bot.SetColorLeds(1, 200, 0, 0);
            bot.SetMotorSpeed(0.5, 0.5);
            bot.SetMotorSpeed(0.5, -0.5);
            bot.SetMotorSpeed(-0.5, 0.5);
            bot.SetMotorSpeed(0, 0);
            while (true)
            {
                var l = bot.ReadLineSensor(true);
                var r = bot.ReadLineSensor(false);
                var v = bot.ReadVoiceLevel();

                Thread.Sleep(50);
                bot.Beep();
            }
        }
        static void TestMaqueen()
        {
            var buzzerController = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.Timer.Pwm.Controller3.P0);
            var lineDetectLeft = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P13);
            var lineDetectRight = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P14);
            var leftHeadlight = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P8);
            var rightHeadight = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P12);

            var bot = new GHIElectronics.TinyCLR.Dfrobot.MicroMaqueen.MaqueenController(
                I2cController.FromName(FEZBit.I2cBus.Edge),
                buzzerChannel,
                leftHeadlight, rightHeadight,
                lineDetectLeft, lineDetectRight,
                GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P15)
                );

            bot.Beep();
            bot.SetColorLeds(1, 100, 0, 0);
            bot.SetColorLeds(0, 0, 50, 100);
            new Thread(() => {
                while (true)
                {
                    bot.SetHeadlight(true, true);
                    Thread.Sleep(200);
                    bot.SetHeadlight(false, true);
                    Thread.Sleep(300);
                }
            }).Start();
            new Thread(() => {
                while (true)
                {
                    bot.Beep();
                    Thread.Sleep(5_000);
                }
            }).Start();

            while (true)
            {
                bot.SetMotorSpeed(0.5, 0.5);
                if (bot.ReadLineSensor(true) || bot.ReadLineSensor(false))
                {
                    bot.SetMotorSpeed(-0.5, -0.5);
                    Thread.Sleep(200);
                    bot.SetMotorSpeed(-0.5, 0.5);
                    Thread.Sleep(200);
                    bot.SetMotorSpeed(0, 0);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(10);
            }
            bot.SetMotorSpeed(0.5, 0.5);
            bot.SetMotorSpeed(0.5, -0.5);
            bot.SetMotorSpeed(-0.5, 0.5);
            bot.SetMotorSpeed(0, 0);
            while (true)
            {
                var l = bot.ReadLineSensor(true);
                var r = bot.ReadLineSensor(false);

                Thread.Sleep(50);
                bot.Beep();
            }

        }
        static void TestYahboomPiano()
        {
            var buzzerController = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzerChannel = buzzerController.OpenChannel(FEZBit.Timer.Pwm.Controller3.P0);

            var piano = new GHIElectronics.TinyCLR.Yahboom.Piano.YahboomPianoController(
                I2cController.FromName(FEZBit.I2cBus.Edge),
                buzzerChannel,
                FEZBit.GpioPin.P1);
            piano.Beep();
            piano.SetColorLeds(0, 50, 100, 0);
            piano.SetColorLeds(1, 50, 0, 100);
            while (true)
            {
                var i = piano.ReadTouch();
                Thread.Sleep(50);
            }

        }
        static void TestScrollBit()
        {
            var scroll = new GHIElectronics.TinyCLR.Pimoroni.ScrollBit.ScrollBitController(I2cController.FromName(FEZBit.I2cBus.Edge));
            var rnd = new Random();

            while (true)
            {
                scroll.SetPixel(rnd.Next(17), rnd.Next(7), rnd.Next(100));
                scroll.Show();
                Thread.Sleep(50);
            }


        }
        static void TestTouchPads()
        {
            var pulse = new PulseFeedback(GpioController.GetDefault().OpenPin(FEZBit.GpioPin.P0),
                PulseFeedbackMode.DrainDuration)
            {
                DisableInterrupts = true,
                Timeout = TimeSpan.FromSeconds(1),
                PulseLength = TimeSpan.FromMilliseconds(1),
                PulseValue = GpioPinValue.High,
                //EchoPinValue = GpioPinValue.High,
                //PulsePinDriveMode = GpioPinDriveMode.Input,
                //EchoPinDriveMode = GpioPinDriveMode.Input
            };

            while (true)
            {
                var d = pulse.Trigger();

                Thread.Sleep(250);
            }
        }
        static void TestWaveshareDisplay()
        {
            var spi = SpiController.FromName(FEZBit.SpiBus.Edge);
            var gpio = GpioController.GetDefault();

            var display = new WaveShare18Display(
                gpio.OpenPin(FEZBit.GpioPin.P8),
                gpio.OpenPin(FEZBit.GpioPin.P12),
                gpio.OpenPin(FEZBit.GpioPin.P16),
                spi,
                gpio.OpenPin(FEZBit.GpioPin.P1));

            display.EnableBacklight(true);

            // Create bitmap buffer
            screen = Graphics.FromImage(new Bitmap(SCREEN_WIDTH, SCREEN_HEIGHT));

            font12 = Resource.GetFont(Resource.FontResources.droid_reg12);
            var i = 0;
            while (true)
            {
                screen.Clear();
                screen.DrawRectangle(new Pen(new SolidBrush(Color.White)), 10, 10, SCREEN_WIDTH - 20, SCREEN_HEIGHT - 20);
                screen.DrawString("FEZ Bit", font12, new SolidBrush(Color.Red), 55, 30);
                screen.DrawString("SITCore", font12, new SolidBrush(Color.Green), 50, 50);
                screen.DrawString(i++.ToString(), font12, new SolidBrush(Color.Blue), 25, 70);
                screen.Flush();
                Thread.Sleep(200);
            }
        }
        static void Main()
        {
            new Thread(Blinker).Start();

            // disable wifi
            var wifien = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.WiFiEnable);
            wifien.SetDriveMode(GpioPinDriveMode.Output);
            wifien.Write(GpioPinValue.High);
            var wifireset = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.WiFiReset);
            wifireset.SetDriveMode(GpioPinDriveMode.Output);
            wifireset.Write(GpioPinValue.High);
            var wifics = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.WiFiChipselect);
            wifics.SetDriveMode(GpioPinDriveMode.Output);
            wifics.Write(GpioPinValue.High);

            //InitDisplay();
            //TestWaveshareDisplay();
            //TestTouchPads();
            //TestYahboomPiano();
            //TestMaqueen();
            TestTinyBit();
            //TestTpBot();
            //TestCuteBot();
            //TestScrollBit();
            //InitBot();


            while (false)
            {
                //bot.SetMotorSpeed(0.5, 0.5);
                //Thread.Sleep(2000);
                //bot.SetMotorSpeed(0.5, -0.5);
                //Thread.Sleep(500);
                //bot.SetMotorSpeed(0, 0);
                //Thread.Sleep(500);
                var d = bot.ReadLineSensor(false);
                bot.Beep();
            }


            // Buzzer ///////////////////
            var pwmController3 = PwmController.FromName(FEZBit.Timer.Pwm.Controller3.Id);
            var buzzer = pwmController3.OpenChannel(FEZBit.Timer.Pwm.Controller3.Buzzer);
            pwmController3.SetDesiredFrequency(500);
            buzzer.SetActiveDutyCyclePercentage(0.5);
            buzzer.Start();
            for (var f = 500; f < 5_000; f += 300)
            {
                pwmController3.SetDesiredFrequency(f);
                Thread.Sleep(1);
            }
            buzzer.Stop();

            bot.SetMotorSpeed(0.5, -0.9);
            screen.DrawString("Press A", font12, new SolidBrush(Color.Teal), 50, 90);
            screen.Flush();
            // wait for A button //////////////////////
            var buttonA = GpioController.GetDefault().OpenPin(FEZBit.GpioPin.ButtonA);
            buttonA.SetDriveMode(GpioPinDriveMode.InputPullUp);
            while (buttonA.Read() == GpioPinValue.High)
            {
                Thread.Sleep(10);
            }
            bot.SetMotorSpeed(0, -0);

            screen.Clear();

            screen.DrawString("TinyCLR OS", font12, new SolidBrush(Color.Teal), 40, 70);
            for (var i = 0; i < 128; i += 8)
            {
                screen.DrawLine(new Pen(Color.Yellow), i, 0, 0, 128 - i);
                bot.Beep();
                screen.Flush();
            }
            for (var i = 0; i < 128; i += 8)
            {
                screen.DrawLine(new Pen(Color.Purple), 160 - i, 0, 160, 128 - i);
                bot.Beep();
                screen.Flush();
            }
            Thread.Sleep(-1);
        }

        private static void Graphics_OnFlushEvent(Graphics sender, byte[] data, int x, int y, int width, int height, int originalWidth)
            => st7735.DrawBuffer(data);
    }
}
