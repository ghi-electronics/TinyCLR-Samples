using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    /// <summary>
    /// info about device/storage that attached to module
    /// </summary>
    public class StorageInfo
    {
        public char  DriveLetter { get; set; }
        public string Name { get; set; }
        public IDriveProvider Drive { get; set; }
        public StorageController Controller { get; set; }
    }
 
}
