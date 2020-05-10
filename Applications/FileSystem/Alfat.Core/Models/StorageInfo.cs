using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    /// <summary>
    /// information about device/storage that mounted to module
    /// </summary>
    public class StorageInfo
    {
        public char  DriveLetter { get; set; }
        public string Name { get; set; }
        public IDriveProvider Drive { get; set; }
        public StorageController Controller { get; set; }
    }
 
}
