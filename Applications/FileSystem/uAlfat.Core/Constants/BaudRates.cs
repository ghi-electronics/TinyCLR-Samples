using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    /// <summary>
    /// Baud rate options
    /// </summary>
    public static class BaudRates
    {
        public static Hashtable ReducedPower { get; set; }
        public static Hashtable FullPower { get; set; }
        static BaudRates()
        {
            FullPower = new Hashtable {
                { "DCEF", 9600 },
                { "6EEF", 19200 },
                { "37EF", 38400 },
                { "43F2", 57600 },
                { "1EF4", 115200 },
                { "0FF4", 230400 },
                { "05A9", 460800 },
                { "028B", 921600 }
            };

            ReducedPower = new Hashtable {
                { "1FAB", 9600 },
                { "0C7C", 19200 },
                { "067C", 38400 },
                { "08E5", 57600 },
                { "04E5", 115200 },
                { "02E5", 230400 },
                { "01E5", 460800 }
            };
            //ReducedPower.Add("-", 921600);

        }
    }
}
