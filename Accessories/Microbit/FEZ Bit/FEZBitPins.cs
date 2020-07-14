namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Bit.</summary>
    public static class FEZBit {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {

            /// <summary>GPIO pin.</summary>
            public const int Led = SC20100.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int WiFiIrq = SC20100.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int WiFiCs = SC20100.GpioPin.PD15;
            /// <summary>GPIO pin.</summary>
            public const int WiFiEn = SC20100.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int WiFiReset = SC20100.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int Backlight = SC20100.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int DisplayCs = SC20100.GpioPin.PD10;
            /// <summary>GPIO pin.</summary>
            public const int DisplayRs = SC20100.GpioPin.PC4;
            /// <summary>GPIO pin.</summary>
            public const int DisplayReset = SC20100.GpioPin.PE15;
            /// <summary>GPIO pin.</summary>
            public const int ButtonLeft = SC20100.GpioPin.PE3;
            /// <summary>GPIO pin.</summary>
            public const int ButtonRight = SC20100.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int ButtonUp = SC20100.GpioPin.PD7;
            /// <summary>GPIO pin.</summary>
            public const int ButtonDown = SC20100.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int ButtonA = SC20100.GpioPin.PE5;
            /// <summary>GPIO pin.</summary>
            public const int ButtonB = SC20100.GpioPin.PE6;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP0 = SC20100.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP1 = SC20100.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP2 = SC20100.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP3 = SC20100.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP4 = SC20100.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP5 = STM32H7.GpioPin.PD13;//SC20100.GpioPin.PD13;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP6 = STM32H7.GpioPin.PD12;// SC20100.GpioPin.PD12;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP7 = STM32H7.GpioPin.PD11;// SC20100.GpioPin.PD11;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP8 = SC20100.GpioPin.PE8;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP9 = SC20100.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP10 = SC20100.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP11 = SC20100.GpioPin.PA13;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP12 = SC20100.GpioPin.PA14;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP13 = SC20100.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP14 = SC20100.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP15 = SC20100.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int EdgeP16 = SC20100.GpioPin.PE7;
        }
        /// <summary>PWM pin definitions.</summary>
        public static class PwmChannel {
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = SC20100.PwmChannel.Controller3.Id;

                /// <summary>PWM pin.</summary>
                public const int BuzzerChannel = SC20100.PwmChannel.Controller3.PB1;
                /// <summary>PWM pin.</summary>
                public const int EdgeP0Channel = SC20100.PwmChannel.Controller3.PC6;
                /// <summary>PWM pin.</summary>
                public const int EdgeP1Channel = SC20100.PwmChannel.Controller3.PC7;
                /// <summary>PWM pin.</summary>
                //public const int EdgeP3Channel = ???
            }
        }
        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = SC20100.AdcChannel.Controller1.Id;

                /// <summary>ADC pin.</summary>
                public const int EdgeP1 = SC20100.AdcChannel.Controller1.PA3;
                /// <summary>ADC pin.</summary>
                public const int EdgeP2 = SC20100.AdcChannel.Controller1.PA5;
                /// <summary>ADC pin.</summary>
                public const int EdgeP3 = SC20100.AdcChannel.Controller1.PA0;
                /// <summary>ADC pin.</summary>
                public const int EdgeP4 = SC20100.AdcChannel.Controller1.PA4;
                /// <summary>ADC pin.</summary>
                public const int EdgeP10 = SC20100.AdcChannel.Controller1.PC0;
            }
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = SC20100.AdcChannel.Controller3.Id;

                /// <summary>ADC pin.</summary>
                public const int EdgeP0 = SC20100.AdcChannel.Controller3.PC2;
                /// <summary>ADC pin.</summary>
                public const int EdgeP9 = SC20100.AdcChannel.Controller3.PC3;
            }
           
        }
        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus.</summary>
            public const string Display = STM32H7.SpiBus.Spi4;
            /// <summary>SPI bus.</summary>
            public const string WiFi = STM32H7.SpiBus.Spi3;
            /// <summary>SPI bus.</summary>
            public const string Edge = STM32H7.SpiBus.Spi3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string Edge = STM32H7.I2cBus.I2c1;
            /// <summary>I2C bus on PB11 (SDA) and PB10 (SCL).</summary>
            public const string Accelerometer = STM32H7.I2cBus.I2c1;
        }
    }
}
