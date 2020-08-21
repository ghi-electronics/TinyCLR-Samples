using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Pins;
using System.Diagnostics;
using System.Threading;

namespace FezBitWeather {
    class Program {
        private static double rainAmount;
        private static int windSpeedCount;

        static void Main() {
            rainAmount = 0;
            windSpeedCount = 0;


            //Rain gauge is connected to P2 (PA5 on FEZ Bit Rev A, PA0 on Rev B).
            var rainGauge = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PA5);
            rainGauge.SetDriveMode(GpioPinDriveMode.InputPullUp);
            rainGauge.ValueChanged += RainGauge_ValueChanged;


            //Wind direction is connected to P1 (PA3).
            var windDirectionPin = SC20100.AdcChannel.Controller1.PA3;
            var windAdcController = AdcController.FromName(SC20100.AdcChannel.Controller1.Id);
            var windDirectionAdc = windAdcController.OpenChannel(windDirectionPin);

            var windDirectionDivisions = new double[] { 0.3715, 0.3845, 0.404, 0.444, 0.4935, 0.535, 0.5955, 0.658, 0.72, 0.7725, 0.802, 0.845, 0.8785, 0.904, 0.9325 };
            var windDirectionText = new string[] { "ESE", "ENE", "E", "SSE", "SE", "SSW", "S", "NNE", "NE", "WSW", "SW", "NNW", "N", "WNW", "NW", "W" };

            var windDirectionAdcCount = windDirectionAdc.ReadRatio();

            var windDirectionIndex = 0;

            for (int i = 0; i < 15; i++) {
                if (windDirectionAdcCount > windDirectionDivisions[i]) {
                    windDirectionIndex++;
                }
            }

            Debug.WriteLine("Wind is out of the: " + windDirectionText[windDirectionIndex]);


            //Wind speed sensor is connected to P8 (PE8).
            var windSpeed = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PE8);
            var windSpeedMeasurementTime = 4; //In seconds.

            windSpeed.SetDriveMode(GpioPinDriveMode.InputPullUp);
            windSpeed.ValueChanged += WindSpeed_ValueChanged;

            windSpeedCount = 0;
            Thread.Sleep(windSpeedMeasurementTime * 1000);
            Debug.WriteLine("Wind speed: " + (windSpeedCount * 1.492 / 4 / windSpeedMeasurementTime).ToString("D1") + " MPH");


            //Humidity/temp sensor is BME280. SDA connected to P20, SCL connected to P19.
            //I2C1 on FEZ Bit.
            //I2C address 0x76.
            //hum_lsb = 0xFE.
            //hum_msb = 0xFD.
            //temp_lsb = 0xFB.
            //temp_msb = 0xFA.
            //press_lsb = 0xF8.
            //press_msb = 0xF7.

            var settings = new I2cConnectionSettings(0x76, 400_000); //The slave's address and the bus speed.
            var controller = I2cController.FromName(SC20100.I2cBus.I2c1);
            var device = controller.GetDevice(settings);

            byte[] writeBuffer = new byte[] { 0xF2, 0x01 }; //must set osrs_h[2:0] to 001. Set humidity to 1x oversampling.

            byte[] readBuffer = new byte[1];

            device.WriteRead(writeBuffer, readBuffer);
            
            writeBuffer[0] = 0xF4; //write 0x27 to 0xF4 (set mode to normal, oversampling for temperature and pressure set to 1X).
            writeBuffer[1] = 0x27;

            device.WriteRead(writeBuffer, readBuffer);

            byte[] writeBuffer2 = new byte[] { 0xF7 };
            byte[] readBuffer2 = new byte[8];

            device.WriteRead(writeBuffer2, readBuffer2);
            
            //Pressure, temperature and humidity are in readBuffer2 array, but must be corrected and calculated using trimming factors.

            var led = GpioController.GetDefault().OpenPin(SC20100.GpioPin.PB0);
            led.SetDriveMode(GpioPinDriveMode.Output);

            while (true) {
                led.Write(GpioPinValue.High);
                Thread.Sleep(100);

                led.Write(GpioPinValue.Low);
                Thread.Sleep(100);

            }
        }

        private static void WindSpeed_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            windSpeedCount++;
        }

        private static void RainGauge_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {
            rainAmount += 0.011 / 2;
            Debug.WriteLine("Rainfall: " + rainAmount.ToString() + " inches");
        }
    }
}
