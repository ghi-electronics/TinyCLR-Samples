namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Bit.</summary>
    public static class FEZBit {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {

            /// <summary>GPIO pin.</summary>
            public const int Led = SC20100.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int Backlight = SC20100.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int DisplayCs = SC20100.GpioPin.PD10;
            /// <summary>GPIO pin.</summary>
            public const int DisplayRs = SC20100.GpioPin.PC4;
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
            public const int EdgeP16 = SC20100.GpioPin.PE7;
        }
        /// <summary>PWM pin definitions.</summary>
        public static class PwmChannel {
            /// <summary>PWM controller.</summary>
            public const string BuzzerController = SC20100.PwmChannel.Controller3.Id;
            /// <summary>PWM pin.</summary>
            public const int BuzzerChannel = SC20100.PwmChannel.Controller3.PB1;
        }
        // <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus.</summary>
            public const string Display = STM32H7.SpiBus.Spi4;
            /// <summary>SPI bus.</summary>
            public const string WiFi = STM32H7.SpiBus.Spi3;
            /// <summary>SPI bus.</summary>
            public const string Edge = STM32H7.SpiBus.Spi3;
        }
    }
}
