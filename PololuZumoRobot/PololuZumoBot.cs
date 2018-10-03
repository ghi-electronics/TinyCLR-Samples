using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Pins;


// schematics https://www.pololu.com/file/0J779/zumo-shield-v1_2-schematic.pdf

namespace Polulou.Zumo
{
    public static class ZumoBot
    {
        private static GpioPin led;
        private static GpioPin button;
        private static AdcChannel voltage;
        private static PwmChannel Buzzer;

        static ZumoBot()
        {
            led = GpioController.GetDefault().OpenPin(FEZ.GpioPin.D13);
            led.SetDriveMode(GpioPinDriveMode.Output);

            button = GpioController.GetDefault().OpenPin(FEZ.GpioPin.D12);
            button.SetDriveMode(GpioPinDriveMode.InputPullUp);

            PwmController pwm = PwmController.FromName(FEZ.PwmChannel.Controller3.Id);
            pwm.SetDesiredFrequency(4 * 1000);
            Buzzer = pwm.OpenPin(FEZ.PwmPin.Controller3.D6); // D3 or D6
            Buzzer.Stop();
            Buzzer.SetActiveDutyCyclePercentage(0.5);

            voltage = AdcController.GetDefault().OpenChannel(FEZ.AdcChannel.A1);
        }
        public static class Gyroscope
        {
            // datasheet https://www.pololu.com/file/0J731/L3GD20H.pdf
            private static I2cDevice device;
            private static byte[] buffer1 = new byte[1];
            private static byte[] buffer2 = new byte[2];

            static Gyroscope()
            {
                var settings = new I2cConnectionSettings(0x6B);//1101011b SA is high
                settings.BusSpeed = I2cBusSpeed.StandardMode;
                settings.SharingMode = I2cSharingMode.Shared;

                //string aqs = I2cDevice.GetDeviceSelector("I2C1");
                device = I2cDevice.FromId(FEZ.I2cBus.I2c1, settings);

                ReadRegister(0x0f);// first read is not working

                if (ReadRegister(0x0f) != 0xD7)// Who am I register
                    throw new Exception("Gyro not found!");

                WriteRegister(0x20, 0x0F);// enable accelerometers CTRL1

            }
            private static void WriteRegister(byte register, byte data)
            {
                buffer2[0] = register;
                buffer2[1] = data;

                device.Write(buffer2);
            }
            private static byte ReadRegister(int register)
            {
                buffer1[0] = (byte)register;
                device.WriteRead(buffer1, buffer1);

                return buffer1[0];
            }
            private static short Read2Complement16bit(int address)
            {
                int l = ReadRegister(address);
                int h = ReadRegister(address + 1);


                var temp = (h << 8);
                temp |= l;

                return (short)temp;
            }
            public static short ReadX() => Read2Complement16bit(0x28);

            public static short ReadY() => Read2Complement16bit(0x2A);

            public static short ReadZ() => Read2Complement16bit(0x2C);

        }
        public static class Accelerometer
        {
            // accel datasheet https://www.pololu.com/file/download/LSM303D.pdf?file_id=0J703
            private static I2cDevice device;
            private static byte[] buffer1 = new byte[1];
            private static byte[] buffer2 = new byte[2];

            static Accelerometer()
            {
                var settings = new I2cConnectionSettings(0x1D) { //0011101b SA is high
                    BusSpeed = I2cBusSpeed.StandardMode,
                };

                //string aqs = I2cDevice.GetDeviceSelector("I2C1");
                device = I2cDevice.FromId(FEZ.I2cBus.I2c1, settings);

                ReadRegister(0x0f);// first read is not working right!!!

                if (ReadRegister(0x0f) != 0x49)// Who am I register
                    throw new Exception("Accelerometer not found!");

                WriteRegister(0x24, 0x80);// enable temp on CTRL5 reg
                WriteRegister(0x20, 0x17);// enable accelerometers CTRL1
            }

            private static void WriteRegister(byte register, byte data)
            {
                buffer2[0] = register;
                buffer2[1] = data;

                device.Write(buffer2);
            }
            private static byte ReadRegister(int register)
            {
                buffer1[0] = (byte)register;
                //byte[] b = new byte[1];
                device.WriteRead(buffer1, buffer1);

                return buffer1[0];
            }
            private static short Read2Complement16bit(int address)
            {
                int l = ReadRegister(address);
                int h = ReadRegister(address + 1);


                var temp = (h << 8);
                temp |= l;

                return (short)temp;
            }
            public static short ReadX() => Read2Complement16bit(0x28);

            public static short ReadY() => Read2Complement16bit(0x2A);

            public static short ReadZ() => Read2Complement16bit(0x2C);

            public static short ReadTemperature()
            {
                int templ = ReadRegister(0x05);
                int temph = ReadRegister(0x06);

                // there is no explanation in datasheet on what the value means except is is 2s complement!
                // plus it says the data is 12 bits but I am seeing 16bits!
                // I am reading -18 and raising teh tempo changes the results
                var temp = (temph << 8);
                temp |= templ;

                return (short)temp;
            }

        }
        public static class Reflectors
        {
            static private GpioPin[] pin;
            static private GpioPin Enable;

            static Reflectors()
            {
                var GPIO = GpioController.GetDefault();
                pin = new GpioPin[6]
                    {
                        GPIO.OpenPin(FEZ.GpioPin.D4),
                        GPIO.OpenPin(FEZ.GpioPin.A3),
                        GPIO.OpenPin(FEZ.GpioPin.D11),
                        GPIO.OpenPin(FEZ.GpioPin.A0),
                        GPIO.OpenPin(FEZ.GpioPin.A2),
                        GPIO.OpenPin(FEZ.GpioPin.D5)
                    };
                Enable = GPIO.OpenPin(FEZ.GpioPin.A4); // D2 or A4
            }

            public static void Activate(bool on)
            {
                if (on)
                    Enable.Write(GpioPinValue.High);
                else
                    Enable.Write(GpioPinValue.Low);
            }

            public static long GetLevel(int reflector)
            {
                pin[reflector].SetDriveMode(GpioPinDriveMode.Output);
                pin[reflector].Write(GpioPinValue.High);
                Thread.Sleep(0);
                var time = DateTime.Now;
                pin[reflector].SetDriveMode(GpioPinDriveMode.Input);
                while (pin[reflector].Read() == GpioPinValue.High)
                    Thread.Sleep(0);

                return DateTime.Now.Ticks - time.Ticks;
            }
        }

        public static class Motors
        {
            private static PwmController PWM = PwmController.FromName(FEZ.PwmChannel.Controller3.Id);
            private static PwmChannel M1PWM, M2PWM;
            private static GpioPin M1DIR, M2DIR;

            static Motors()
            {
                PWM.SetDesiredFrequency(6000);

                M1PWM = PWM.OpenPin(FEZ.PwmPin.Controller3.D9);
                M1PWM.Stop();
                M1PWM.SetActiveDutyCyclePercentage(0.1);
                M1PWM.Start();

                M2PWM = PWM.OpenPin(FEZ.PwmPin.Controller3.D10);
                M2PWM.Stop();
                M2PWM.SetActiveDutyCyclePercentage(0.1);
                M2PWM.Start();

                M1DIR = GpioController.GetDefault().OpenPin(FEZ.GpioPin.D7);
                M1DIR.SetDriveMode(GpioPinDriveMode.Output);
                M1DIR.Write(GpioPinValue.High);

                M2DIR = GpioController.GetDefault().OpenPin(FEZ.GpioPin.D8);
                M2DIR.Write(GpioPinValue.High);
                M2DIR.SetDriveMode(GpioPinDriveMode.Output);
            }

            public static void MoveForward() => Move(80, 80);

            public static void MoveBackward() => Move(-80, -80);

            public static void TurnRight() => Move(50, -50);

            public static void TurnLeft() => Move(-50, 50);

            public static void Stop() => Move(0, 0);

            public static void Move(double LeftSpeed, double RightSpeed)
            {
                if (RightSpeed < 0)
                    M1DIR.Write(GpioPinValue.High);
                else
                    M1DIR.Write(GpioPinValue.Low);

                if (LeftSpeed < 0)
                    M2DIR.Write(GpioPinValue.High);
                else
                    M2DIR.Write(GpioPinValue.Low);

                M1PWM.SetActiveDutyCyclePercentage(Math.Abs(RightSpeed) / 100);
                M2PWM.SetActiveDutyCyclePercentage(Math.Abs(LeftSpeed) / 100);
            }
        }

        public static void Beep(int duration)
        {
            Buzzer.Start();
            Thread.Sleep(duration);
            Buzzer.Stop();
        }

        public static void Beep() => Beep(10);

        public static bool ButtonIsPressed() => button.Read() == GpioPinValue.Low;

        public static double BatteryVoltage() => voltage.ReadRatio() * 3.3 / 0.6;

        public static void SetLed(bool on)
        {
            if (on)
                led.Write(GpioPinValue.High);
            else
                led.Write(GpioPinValue.Low);
        }
    }
}