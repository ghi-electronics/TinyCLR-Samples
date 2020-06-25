using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    /// <summary>
    /// file handle
    /// </summary>
    public class MediaHandle
    {
        public char HandleName { get; set; }
        //public char AccessType { get; set; }
        public string FileName { get; set; }
        public string Media { get; set; }
      
        /// <summary>
        /// buffer for reading/writing file
        /// </summary>
        public FileStream Buffer { get; set; }
        /// <summary>
        /// pointer
        /// </summary>
        public long CursorPosition => this.Buffer == null ? 0 : this.Buffer.Position;


        public long Size => this.Buffer == null ? 0 : this.Buffer.Length;
        public MediaHandle() {
        }

        public FileMode AccessMode { get; set; }

    }
}
