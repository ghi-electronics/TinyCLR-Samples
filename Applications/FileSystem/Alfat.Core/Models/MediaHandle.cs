using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class MediaHandle
    {
        public char HandleName { get; set; }
        public char AccessType { get; set; }
        public string FileName { get; set; }
        public string Media { get; set; }
        /// <summary>
        /// for storing data from/to file 
        /// </summary>
        public MemoryStream Buffer { get; set; }
        /// <summary>
        /// data pointer
        /// </summary>
        public long CursorPosition { get; set; }
        public long Size { get; set; }
        public MediaHandle()
        {
            this.Buffer = new MemoryStream();
            this.CursorPosition = 0;
        }

    }
}
