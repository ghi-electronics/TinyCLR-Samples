using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class Console
    {
        public static void WriteLine(string Message,params string[] Params)
        {
            if (Params.Length > 0)
                Debug.WriteLine(string.Format(Message,Params));
            else
                Debug.WriteLine(Message);
        }
    }
}
