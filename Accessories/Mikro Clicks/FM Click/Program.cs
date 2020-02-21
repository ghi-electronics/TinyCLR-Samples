using GHIElectronics.TinyCLR.Pins;


namespace Radio_Click {
    class Program {

        public static int reset = SC20100.GpioPin.PD4;
        public static int cs = SC20100.GpioPin.PD3;
        public static string i2cbus = SC20100.I2cBus.I2c1;
        private static void Main() {
            Click.Radio.RadioFM1 radio = new Click.Radio.RadioFM1(reset,cs,i2cbus);
            double currentStation = 101.9;
            int volume = 255;
            radio.Channel = currentStation;
            radio.Volume = volume;
        }
    }
}
        

