using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Native;
using System;
using System.Diagnostics;
using System.Threading;

namespace RealTimeClock {
    class Program {
        static void Main() {
            var rtc = RtcController.GetDefault();
            if (rtc.IsValid) {
                Debug.WriteLine("RTC is Valid");
                // RTC is good so let's use it
                SystemTime.SetTime(rtc.Now);
            }
            else {
                Debug.WriteLine("RTC is Invalid");
                // RTC is not valid. Get user input to set it
                // This example will simply set it to January 1st 2019 at 11:11:11
                var MyTime = new DateTime(2019, 1, 1, 11, 11, 11);
                rtc.Now = MyTime;
                SystemTime.SetTime(MyTime);
            }

            while (true) {
                Debug.WriteLine("Current Time    : " + DateTime.Now);
                Debug.WriteLine("Current RTC Time: " + rtc.Now);
                Thread.Sleep(1000);
            }
        }
    }
}