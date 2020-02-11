using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Uart;

namespace Gps
{
    class Program
    {
        static void Main()
        {

            var serial = UartController.FromName("GHIElectronics.TinyCLR.NativeApis.STM32H7.UartController\\2");

            var resetPin = 3 * 16 + 15;

            var gps = new Gps(serial, resetPin);

            gps.InvalidPositionReceived += Gps_InvalidPositionReceived;
            gps.NmeaSentenceReceived += Gps_NmeaSentenceReceived;
            gps.PositionReceived += Gps_PositionReceived;

            Debug.WriteLine("staring");

            gps.Enabled = true; // Start Gps

            Debug.WriteLine("Enabled");

            Thread.Sleep(-1);
        }

        private static void Gps_PositionReceived(Gps sender, Gps.Position e) => Debug.WriteLine(e.ToString());

        private static void Gps_NmeaSentenceReceived(Gps sender, string e) => Debug.WriteLine("Nmea : " + e);

       private static void Gps_InvalidPositionReceived(Gps sender, EventArgs e) => Debug.WriteLine("InvalidPosition");
    }
}
