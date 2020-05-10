using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class Console
    {
        public static void WriteLine(string message,params string[] parammeters)
        {
            if (parammeters.Length > 0)
                Debug.WriteLine(string.Format(message, parammeters));
            else
                Debug.WriteLine(message);
        }
    }
}
