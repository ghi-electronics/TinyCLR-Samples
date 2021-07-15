using System;
using System.Diagnostics;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Microchip.Mcp23xxx.Device;
using GHIElectronics.TinyCLR.Pins;

using static GHIElectronics.TinyCLR.Drivers.Microchip.Mcp23xxx.Mcp23Xxx;

// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles


namespace Microchip
{
    static class Program
    {
        private static Timer _outTimer;

        /// <summary>
        /// Using the Microchip Mcp23S18 gpio expander as a native like TinyClr Gpio Pin
        /// </summary>
        private static void Main()
        {
            // use default GPIO controller to setup pins for Mcp23xxx IO Expander
            var gpioController = GpioController.GetDefault();
            var reset = gpioController.OpenPin(SC13048.GpioPin.PA2);
            var chipSelect = gpioController.OpenPin(SC13048.GpioPin.PA3);
            var interruptPin = gpioController.OpenPin(SC13048.GpioPin.PB8);
            interruptPin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            // create a Microchip MCP23S18 GPIO expander provider using the SPI bus on the FLEA
            var mcp23S18 = new Mcp23GpioProvider(Product.Mcp23X18, SpiController.FromName(SC13048.SpiBus.Spi2), chipSelect, reset: reset, interruptPin: interruptPin);
            // use the Mcp23S18 provider to create a 'native' GpioController
            var externalGpioController = GpioController.FromProvider(mcp23S18);

            // use the external GpioController to set up an external LED
            // use port A, pin 0, set as output, no pull-up (using external pull up resistor as Mcp23S18 has open drain outputs)
            var exLed = externalGpioController.OpenPin(ExternalGpioPin.GpA0);
            exLed.SetDriveMode(GpioPinDriveMode.OutputOpenDrain);

            // use the external GpioController to set up an external Button
            // use port A, pin 1 set as input, using internal pull-up and 30ms debounce timing, set with a callback when either rising or falling edge is detected
            var exButton = externalGpioController.OpenPin(ExternalGpioPin.GpA1);
            exButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            exButton.DebounceTimeout = TimeSpan.FromMilliseconds(30);
            exButton.ValueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;
            exButton.ValueChanged += (_, args) =>
            {
                Debug.WriteLine($"change: edge:{(args.Edge == GpioPinEdge.RisingEdge ? "rise" : "fall")} ts:{args.Timestamp}");
                exLed.Write(exLed.Read() == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);
            };

            // now you can use an expanded GPIO pin wherever a standard GpioPin is required; without need to extend the sealed GpioPin class
            var exChipSelect = externalGpioController.OpenPin(ExternalGpioPin.GpB0);
            exChipSelect.SetDriveMode(GpioPinDriveMode.Output);
            var s = SpiController.FromName(SC13048.SpiBus.Spi1);
            s.GetDevice(new SpiConnectionSettings{ ChipSelectType = SpiChipSelectType.Gpio, ChipSelectLine = exLed });


            // finally use default GPIO controller to blink the internal LED and ensure things are running as expected :-)
            var onBoardLed = gpioController.OpenPin(SC13048.GpioPin.PA8);
            onBoardLed.SetDriveMode(GpioPinDriveMode.Output);

            _outTimer = new Timer(_ =>
            {
                onBoardLed.Write(onBoardLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                _outTimer.Change(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));

            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));

            // put main thread to sleep
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
