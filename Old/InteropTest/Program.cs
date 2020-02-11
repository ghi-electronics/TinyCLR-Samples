using System;
using System.Threading;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Native;
using System.Diagnostics;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace InteropTest {
    class Program {
        static void Blinker() {
            var LED = GpioController.GetDefault().OpenPin(FEZ.GpioPin.Led1);
            LED.SetDriveMode(GpioPinDriveMode.Output);
            while (true) {
                LED.Write(GpioPinValue.High);
                Thread.Sleep(100);
                LED.Write(GpioPinValue.Low);
                Thread.Sleep(100);
            }
        }
        static void Main() {
            new Thread(Blinker).Start();

            const uint RLI_ADDRESS = 0x20016000;

            //copy native interop to RLI
            var interop =  Resource.GetBytes(Resource.BinaryResources.Interop);
            Marshal.Copy(interop, 0, new IntPtr(RLI_ADDRESS), interop.Length);
            interop = null;
            System.GC.Collect();

            // Let the CLR know about it
            // You normally need ot go in the MAP file of the finary to find the location of the table
            // In this example, we modified the "scatter file" to position the table at the base
            // So, in this case base address of RLI (Runtime Loadable Interops) is the same as interop table
            Interop.Add(new IntPtr(RLI_ADDRESS));

            // Test XTEA
            Debug.WriteLine(Environment.NewLine + "Test XTEA is working.");
            var Data = new uint[2] { 0x12345678, 0x98765432 }; // 8-byte block
            var Key = new uint[4] { 0x11111111, 0x22222222, 0x33333333, 0x44444444 }; // 128-bit key
            Debug.WriteLine("Original Data    : " + Data[0].ToString("X") + " " + Data[1].ToString("X"));
            Cipher.Xtea.EncipherFast(32, Data, Key);
            Debug.WriteLine("Fast Encoded Data: " + Data[0].ToString("X") + " " + Data[1].ToString("X"));
            Cipher.Xtea.DecipherSlow(32, Data, Key);
            Debug.WriteLine("Slow Decoded Data: " + Data[0].ToString("X") + " " + Data[1].ToString("X"));
            Cipher.Xtea.EncipherSlow(32, Data, Key);
            Debug.WriteLine("Slow Encoded Data: " + Data[0].ToString("X") + " " + Data[1].ToString("X"));
            Cipher.Xtea.DecipherFast(32, Data, Key);
            Debug.WriteLine("Fast Decoded Data: " + Data[0].ToString("X") + " " + Data[1].ToString("X"));

            // Compare speed
            Debug.WriteLine(Environment.NewLine + "Speed Test for 1000 native enc/dec loops.");
            var T = DateTime.Now;
            for (var i = 0; i < 1000; i++) {
                Cipher.Xtea.EncipherFast(32, Data, Key);
                Cipher.Xtea.DecipherFast(32, Data, Key);
            }
            var DeltaNativeTime = DateTime.Now - T;
            Debug.WriteLine("Time -> " + (DeltaNativeTime.TotalMilliseconds / 1000).ToString("F2") + " Seconds.");

            Debug.WriteLine("Speed Test for 1000 managed enc/dec loops.");
            T = DateTime.Now;
            for (var i = 0; i < 1000; i++) {
                Cipher.Xtea.EncipherSlow(32, Data, Key);
                Cipher.Xtea.DecipherSlow(32, Data, Key);
            }
            var DeltaManagedTime = DateTime.Now - T;
            Debug.WriteLine("Time -> " + (DeltaManagedTime.TotalMilliseconds / 1000).ToString("F2") + " Seconds.");

            Debug.WriteLine(Environment.NewLine + "Native was " + (DeltaManagedTime.TotalMilliseconds / DeltaNativeTime.TotalMilliseconds).ToString("F0") + " times faster than managed.");

            Thread.Sleep(Timeout.Infinite);

        }
    }
}
