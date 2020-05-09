using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    /// <summary>
    /// listing file result, it can be a file or a folder
    /// </summary>
    public class ItemResult
    {
        public enum ResultTypes { File, Folder };
        public ResultTypes ResultType { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public int Attribute { get; set; }
        public DateTime LastModified { get; set; }

        public static int GetAttribute(FileAttributes attr)
        {
            switch (attr)
            {
                case FileAttributes.ReadOnly:
                    return 0;
                case FileAttributes.Hidden:
                    return 1;
                case FileAttributes.System:
                    return 2;
                case FileAttributes.Normal:
                    return 3;
                case FileAttributes.Directory:
                    return 4;
                case FileAttributes.Archive:
                    return 5;
                default:
                    return 6;
            }
        }
    
    }
}
