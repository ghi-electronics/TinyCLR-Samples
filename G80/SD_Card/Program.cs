using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Native;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SD_Card {
    public class Program {
        private static void Main() {
            // Indicator LED
            var led1 = GpioController.GetDefault().OpenPin(G80.GpioPin.PE14);
            led1.SetDriveMode(GpioPinDriveMode.Output);
            led1.Write(GpioPinValue.Low);

            var sd = StorageController.FromName( @"GHIElectronics.TinyCLR.NativeApis.STM32F4.SdCardStorageController\0");
            var drive = FileSystem.Mount(sd.Hdc);

            // While not required, having the right time is needed
            // The File System saves the file with current time
            var rtcControl = RtcController.GetDefault();
            // We will use this time if RTC is not valid - 2019/January/1 10:5:0
            var CurrentTime = new DateTime(2019, 1, 1, 10, 5, 0);
            if (rtcControl.IsValid)
                CurrentTime = rtcControl.Now;
            else
                rtcControl.Now = CurrentTime;
            SystemTime.SetTime(CurrentTime);
            // Add a file
            var file = new FileStream($@"{drive.Name}Test.txt", FileMode.Append);
            var bytes = Encoding.UTF8.GetBytes("Hello is recorded at: " +
                DateTime.UtcNow.ToString() + Environment.NewLine);
            file.Write(bytes, 0, bytes.Length);
            file.Close();

            // Show a list of files on root directory
            var directory = new DirectoryInfo(drive.Name);
            var files = directory.GetFiles();
            foreach (var f in files) {

                Debug.WriteLine(f.Name);
            }

            FileSystem.Flush(sd.Hdc);

            // Turn an LED on indicating the operation is complete!
            // Reset the board a few times and check the file content.
            led1.Write(GpioPinValue.High);
        }
    }
}