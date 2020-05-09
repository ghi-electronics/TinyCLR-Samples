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
            FullPower = new Hashtable();
            FullPower.Add("DCEF", 9600);
            FullPower.Add("6EEF", 19200);
            FullPower.Add("37EF", 38400);
            FullPower.Add("43F2", 57600);
            FullPower.Add("1EF4", 115200);
            FullPower.Add("0FF4", 230400);
            FullPower.Add("05A9", 460800);
            FullPower.Add("028B", 921600);

            ReducedPower = new Hashtable();
            ReducedPower.Add("1FAB", 9600);
            ReducedPower.Add("0C7C", 19200);
            ReducedPower.Add("067C", 38400);
            ReducedPower.Add("08E5", 57600);
            ReducedPower.Add("04E5", 115200);
            ReducedPower.Add("02E5", 230400);
            ReducedPower.Add("01E5", 460800);
            //ReducedPower.Add("-", 921600);

        }
    }
}
