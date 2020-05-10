using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class ActiveHandle
    {
        public enum HandleMode { Idle, Writing };
        public HandleMode Mode { get; set; }
        public MediaHandle Handle { get; set; }
    }
}
