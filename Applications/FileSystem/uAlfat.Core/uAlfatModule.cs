using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using uAlfat.Core.Properties;

namespace uAlfat.Core {
    public class uAlfatModule {

        public enum PowerModes {
            Full = 'F', Reduced = 'R', Hibernate = 'H'
        }
        public string CurrentPath { get; set; }
        public PowerModes PowerMode { get; set; }
        public int LDRPin { get; set; }
        public static bool IsEchoEnabled { get; set; }
        public TimerModes TimerMode { get; set; }
        public enum TimerModes { Shared = 'S', Backup = 'B' };
        static FileExplorer fileExplorer;
        static ActiveHandle activeHandle;
        static MediaHandler handles;
        static StorageContainer storages;
        string SDControllerName { get; set; }
        public bool IsUsbDiskConnected { get; set; }
        public bool IsUsbDiskInitialized { get; set; }
        public bool IsUsbHostInitialized { get; set; }
        public bool IsSDConnected { get; set; }
        public bool IsKeyboardConnected { get; set; }
        string StorageControllerName { get; set; }
        public static GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController UsbHost { set; get; }
        public static CommunicationsBus Bus { get; set; }

        public byte[] dataBlock;
        const int DataBlockSize = 4 * 1024;
        const int UsbHostConnectionTimeoutMillisecond = 2000;
        const int MaxHandle = 4;

        const string VersionNumber = "   uALFAT(TM) 3.13";

        public uAlfatModule(string uartPort, string storageControllerName, string sDControllerName, int ldrPin = SC20260.GpioPin.PE3) {
            this.CurrentPath = string.Empty;
            this.PowerMode = PowerModes.Full;
            this.LDRPin = ldrPin;
            IsEchoEnabled = false;
            this.TimerMode = TimerModes.Backup;
            fileExplorer = new FileExplorer();
            activeHandle = new ActiveHandle() { Mode = ActiveHandle.HandleMode.Idle };
            handles = new MediaHandler();
            storages = new StorageContainer();
            Bus = new CommunicationsBus(uartPort);
            this.StorageControllerName = storageControllerName;
            this.SDControllerName = sDControllerName;
            this.dataBlock = new byte[DataBlockSize];

            //Console.WriteLine("uAlfat is ready");
            this.PrintStartUpMessage();
        }

        void PrintStartUpMessage() {
            var appVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var bootVer = Resources.GetString(Resources.StringResources.BOOTLOADER_VER);
            Bus.WriteLine($" GHI Electronics, LLC{Strings.NewLine}----------------------{Strings.NewLine}   Boot Loader 2.05{Strings.NewLine}{VersionNumber}{Strings.NewLine}{ResponseCode.Success}");
            //Console.WriteLine($" GHI Electronics, LLC{Strings.NewLine}----------------------{Strings.NewLine}   Boot Loader 2.05{Strings.NewLine}   uALFAT(TM) 3.13{Strings.NewLine}{ResponseCode.Success}"); 
        }

        public void Run() {
            var cmd = string.Empty;

            while (true) {
                var dataToRead = Bus.ByteToRead;

                if (dataToRead > 0) {
                    dataToRead = dataToRead < Bus.ReadBufferSize ? dataToRead : Bus.ReadBufferSize;

                    var data = new byte[dataToRead];
                    Bus.Read(data);

                    var dataStr = Encoding.UTF8.GetString(data, 0, dataToRead);

                    if (uAlfatModule.IsEchoEnabled) {
                        for (var i = 0; i < data.Length; i++) {
                            // send back whatever the host sent except for terminal line                    
                            Bus.Write(data, i, 1);
                        }
                    }

                    cmd += dataStr;

                    if (dataStr.IndexOf(Strings.NewLine) > -1) {
                        var trim = cmd.Trim();

                        this.ProcessCommand(trim);

                        cmd = string.Empty;
                    }
                }
                else {
                    Thread.Sleep(1);
                }
            }
        }

        private void ProcessCommand(string data) {
            var isSuccess = false;
            var result = string.Empty;

            var cmd = CommandParser.Parse(data);
            string[] par1;

            //alfat commands
            switch (cmd.CommandPrefix) {
                case CommandTypes.Init: {
                        //try connect sd card
                        isSuccess = this.ConnectSD(MediaTypes.D);
                        if (isSuccess) {
                            result = ResponseCode.Success;
                        }
                        else {
                            result = ResponseCode.NoSDCard;
                        }
                        if (!isSuccess) {
                            //try connect usb disk
                            if (this.IsUsbDiskConnected) {
                                isSuccess = true;
                                result = ResponseCode.Success;
                            }
                            else {
                                result = ResponseCode.NoSDCard;
                            }
                        }
                        if (isSuccess)
                            Bus.WriteLine(ResponseCode.Success);
                        else
                            Bus.WriteLine(ResponseCode.NoSDCard);
                    }
                    break;
                case CommandTypes.MountUsb: {
                        if (!this.IsUsbHostInitialized) {
                            this.InitUsbHost();
                        }

                        //try connect usb disk
                        if (this.IsUsbDiskConnected) {

                            try {
                                var storageController = StorageController.FromName(this.StorageControllerName);
                                IDriveProvider driver;

                                if (this.IsUsbDiskInitialized) {
                                    for (var h = 0; h < MaxHandle; h++) {
                                        var handle = (char)(h + '0');

                                        if (handles.IsExist(handle)) {
                                            //if write/append mode then flush
                                            var currentHandle = handles.GetHandle(handle);

                                            var storage = storages.GetStorage(currentHandle.Media);

                                            currentHandle.Buffer.Flush();

                                            FileSystem.Flush(storage.Controller.Hdc);

                                            currentHandle.Buffer.Close();

                                            currentHandle.Buffer.Dispose();
                                            var res = handles.RemoveHandle(handle);
                                            result = res ? ResponseCode.Success : ResponseCode.InvalidHandle;
                                        }
                                    }

                                    FileSystem.Unmount(storageController.Hdc);

                                    this.IsUsbDiskInitialized = false;

                                    Thread.Sleep(1);
                                }

                                driver = GHIElectronics.TinyCLR.IO.FileSystem.
                                    Mount(storageController.Hdc);

                                if (driver != null) {
                                    var driveInfo = new System.IO.DriveInfo(driver.Name);
                                    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U0 });
                                    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U1 });
                                    if (string.IsNullOrEmpty(this.CurrentPath))
                                        this.CurrentPath = driveInfo.RootDirectory.FullName;
                                    //System.Diagnostics.Debug.WriteLine
                                    //    ("Free: " + driveInfo.TotalFreeSpace);

                                    //System.Diagnostics.Debug.WriteLine
                                    //    ("TotalSize: " + driveInfo.TotalSize);

                                    //System.Diagnostics.Debug.WriteLine
                                    //    ("VolumeLabel:" + driveInfo.VolumeLabel);

                                    //System.Diagnostics.Debug.WriteLine
                                    //    ("RootDirectory: " + driveInfo.RootDirectory);

                                    //System.Diagnostics.Debug.WriteLine
                                    //    ("DriveFormat: " + driveInfo.DriveFormat);

                                    this.IsUsbDiskInitialized = true;
                                    isSuccess = true;
                                    result = ResponseCode.Success;
                                }
                            }
                            catch (Exception ex) {
                                //ummount -> then mount
                                //FileSystem.Unmount(storageController.Hdc);
                                //driver = GHIElectronics.TinyCLR.IO.FileSystem.
                                //  Mount(storageController.Hdc);
                                this.IsUsbDiskInitialized = false;
                                result = ResponseCode.ERROR_USB_INITIALIZE_FAILED;
                            }



                        }
                        else {
                            this.IsUsbDiskInitialized = false;
                            result = ResponseCode.NoSDCard;
                        }

                        if (isSuccess)
                            Bus.WriteLine(ResponseCode.Success);
                        else
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_NO_USB);
                    }
                    break;
                case CommandTypes.DetectUsb: {
                        if (!this.IsUsbHostInitialized) {
                            this.InitUsbHost();
                        }

                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        //try connect usb disk
                        if (this.IsUsbDiskConnected) {
                            isSuccess = true;
                            result += "$01" + Strings.NewLine;
                        }
                        else {
                            result += "$00" + Strings.NewLine;
                        }
                        result += ResponseCode.Success;

                        Bus.WriteLine(result);
                    }
                    break;
                case CommandTypes.Open:
                    if (cmd.ParamLength > 0) {
                        par1 = cmd.Parameters[0].Trim().Split('>');

                        if (par1.Length > 0) {
                            try {
                                var fileName = par1[1];
                                var handle = par1[0][0];
                                var accessType = par1[0][1];

                                //is handle available
                                if (handles.IsExist(handle)) {
                                    result = ResponseCode.HandleAlreadyUsed;
                                }
                                else {
                                    //has storage been init ?
                                    for (var i = 0; i < storages.Size; i++) {
                                        var storage = storages.GetStorageByIndex(i);
                                        if (storage != null) {
                                            fileName = $"{this.CurrentPath}{fileName}";
                                            var newHandle = new MediaHandle() { HandleName = handle, FileName = fileName, Media = storage.Name };

                                            switch (accessType) {
                                                case FileAccessTypes.Read:

                                                    if (File.Exists(fileName)) {
                                                        newHandle.AccessMode = FileMode.Open;

                                                        newHandle.Buffer = new FileStream(fileName, newHandle.AccessMode);

                                                        result = ResponseCode.Success;
                                                    }
                                                    else {
                                                        result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                                    }
                                                    break;
                                                case FileAccessTypes.Write:

                                                    newHandle.AccessMode = FileMode.Create;

                                                    try {
                                                        newHandle.Buffer = new FileStream(fileName, newHandle.AccessMode);
                                                        result = ResponseCode.Success;
                                                    }

                                                    catch {
                                                        result = ResponseCode.ERROR_FAILED_OPEN_FILE;
                                                    }

                                                    break;
                                                case FileAccessTypes.Append:

                                                    if (File.Exists(fileName)) {
                                                        newHandle.AccessMode = FileMode.Append;

                                                        newHandle.Buffer = new FileStream(fileName, newHandle.AccessMode);
                                                        //set to end of file
                                                        newHandle.Buffer.Seek(0, SeekOrigin.End);
                                                        result = ResponseCode.Success;
                                                    }
                                                    else {
                                                        result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                                    }
                                                    break;
                                            }
                                            if (result == ResponseCode.Success) {
                                                handles.AddHandle(newHandle);
                                            }

                                            break;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(result)) result = ResponseCode.MediaNotInitialize;


                                }
                            }
                            catch {
                                result = ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER;
                            }

                            Bus.WriteLine(result);
                        }
                        else {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Flush:
                    if (cmd.ParamLength > 0) {
                        var handle = cmd.Parameters[0].Trim()[0];

                        if (handles.IsExist(handle)) {
                            //if write/append mode then flush
                            result = ResponseCode.Success;

                            var currentHandle = handles.GetHandle(handle);

                            var storage = storages.GetStorage(currentHandle.Media);

                            currentHandle.Buffer.Flush();

                            FileSystem.Flush(storage.Controller.Hdc);
                        }
                        else {
                            result = ResponseCode.InvalidHandle;
                        }
                        Bus.WriteLine(result);

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Close:
                    if (cmd.ParamLength > 0) {
                        var handle = cmd.Parameters[0].Trim()[0];

                        if (handles.IsExist(handle)) {
                            //if write/append mode then flush
                            var currentHandle = handles.GetHandle(handle);

                            var storage = storages.GetStorage(currentHandle.Media);

                            currentHandle.Buffer.Flush();

                            FileSystem.Flush(storage.Controller.Hdc);

                            currentHandle.Buffer.Close();

                            currentHandle.Buffer.Dispose();
                            var res = handles.RemoveHandle(handle);
                            result = res ? ResponseCode.Success : ResponseCode.InvalidHandle;
                        }
                        else {
                            result = ResponseCode.InvalidHandle;
                        }
                        Bus.WriteLine(result);

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Tell:
                    if (cmd.ParamLength > 0) {
                        var handle = cmd.Parameters[0].Trim()[0];
                        var currentHandle = handles.GetHandle(handle);
                        if (currentHandle != null) {
                            result = $"{ResponseCode.Success}{Strings.NewLine}";
                            //write current read position
                            var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", currentHandle.CursorPosition), 8);
                            result += $"{actBytes}{Strings.NewLine}";
                            result += ResponseCode.Success;
                        }
                        else {
                            result = ResponseCode.InvalidHandle;
                        }
                        Bus.WriteLine(result);

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Read:
                    if (cmd.ParamLength > 0) {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0) {
                            try {
                                var numReadBytes = Convert.ToInt32("0x" + par1[1], 16);
                                var handle = par1[0][0];
                                var pad = par1[0][1];
                                var currentHandle = handles.GetHandle(handle);
                                //is handle available
                                if (currentHandle == null) {
                                    result = ResponseCode.InvalidHandle;
                                }
                                else if (currentHandle.AccessMode != FileMode.Open) {
                                    result = ResponseCode.HandleRequireRead;
                                }
                                else {

                                    //has storage been init ?
                                    var storage = storages.GetStorage(currentHandle.Media);
                                    if (storage == null) {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else {
                                        if (currentHandle.Buffer.Length > 0) {
                                            result = ResponseCode.Success + Strings.NewLine;

                                            Bus.WriteLine(result);

                                            var actualByteToRead = (int)Math.Min(currentHandle.Buffer.Length, numReadBytes);
                                            var numberOfPad = numReadBytes - actualByteToRead;

                                            var block = actualByteToRead / DataBlockSize;
                                            var remain = actualByteToRead % DataBlockSize;

                                            while (block > 0) {
                                                currentHandle.Buffer.Read(this.dataBlock, 0, DataBlockSize);
                                                Bus.Write(this.dataBlock);
                                                block--;
                                            }

                                            if (remain > 0) {
                                                currentHandle.Buffer.Read(this.dataBlock, 0, remain);
                                                Bus.Write(this.dataBlock, 0, remain);

                                            }

                                            if (numberOfPad > 0) {
                                                var dataPad = new byte[1] { (byte)pad };
                                                for (var i = 0; i < numberOfPad; i++) {
                                                    Bus.Write(dataPad);

                                                }
                                            }

                                            var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", actualByteToRead), 8);
                                            result = $"{actBytes}{Strings.NewLine}";

                                            result += ResponseCode.Success;
                                        }
                                        else {
                                            result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                        }


                                    }
                                }
                            }
                            catch {
                                result = ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER;
                            }

                            Bus.WriteLine(result);
                        }
                        else {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Write:
                    if (cmd.ParamLength > 0) {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0) {
                            try {
                                var handle = par1[0][0];
                                var numWriteBytes = Convert.ToInt32("0x" + par1[1], 16);

                                var currentHandle = handles.GetHandle(handle);
                                //is handle available
                                if (currentHandle == null) {
                                    result = ResponseCode.InvalidHandle;
                                }
                                else if (currentHandle.AccessMode != FileMode.Create
                                    && currentHandle.AccessMode != FileMode.OpenOrCreate
                                    && currentHandle.AccessMode != FileMode.Append
                                    && currentHandle.AccessMode != FileMode.CreateNew
                                    ) {
                                    result = ResponseCode.HandleRequireAppend;
                                }
                                else {
                                    //has storage been init ?
                                    var storage = storages.GetStorage(currentHandle.Media);
                                    if (storage == null) {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else {
                                        result = ResponseCode.Success;
                                        Bus.WriteLine(result);

                                        var block = numWriteBytes / DataBlockSize;
                                        var remain = numWriteBytes % DataBlockSize;

                                        while (block > 0) {
                                            // Read data
                                            Bus.Read(this.dataBlock);

                                            // Write to file
                                            currentHandle.Buffer.Write(this.dataBlock, 0, this.dataBlock.Length);

                                            block--;
                                        }

                                        if (remain > 0) {

                                            Bus.Read(this.dataBlock, 0, remain);
                                            currentHandle.Buffer.Write(this.dataBlock, 0, remain);
                                        }


                                        var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", numWriteBytes), 8);

                                        result = actBytes + Strings.NewLine;
                                        result += ResponseCode.Success;
                                    }
                                }
                            }
                            catch {
                                result = ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER;
                            }

                            Bus.WriteLine(result);
                        }
                        else {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Seek:
                    if (cmd.ParamLength > 0) {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0) {
                            try {
                                var handle = par1[0][0];
                                long newPosition = Convert.ToInt32("0x" + par1[1], 16);

                                var currentHandle = handles.GetHandle(handle);
                                //is handle available
                                if (currentHandle == null) {
                                    result = ResponseCode.InvalidHandle;
                                }
                                else if (currentHandle.AccessMode != FileMode.Open) {
                                    result = ResponseCode.HandleRequireRead;
                                }
                                else {
                                    //has storage been init ?
                                    var storage = storages.GetStorage(currentHandle.Media);
                                    if (storage == null) {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else {
                                        //if new position > file size, go to EOF
                                        //if (newPosition > currentHandle.Size)
                                        //    newPosition = currentHandle.Size;

                                        //currentHandle.CursorPosition = newPosition;

                                        if (currentHandle.AccessMode != FileMode.Open) {
                                            result = ResponseCode.ERROR_FS_SEEK_READ_ONLY;
                                        }
                                        else if (newPosition + currentHandle.Buffer.Position > currentHandle.Buffer.Length) {
                                            result = ResponseCode.ERROR_FS_SEEK_OUTOF_LENGTH;
                                        }
                                        else {
                                            currentHandle.Buffer.Seek(newPosition, SeekOrigin.Begin);
                                            result = ResponseCode.Success;
                                        }
                                    }
                                }
                            }
                            catch {
                                result = ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER;
                            }

                            Bus.WriteLine(result);
                        }
                        else {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.Delete:
                    if (cmd.ParamLength > 0) {
                        try {
                            var fileName = cmd.Parameters[0];

                            for (var i = 0; i < storages.Size; i++) {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null) {
                                    fileName = this.CurrentPath + fileName;
                                    if (File.Exists(fileName)) {
                                        var isExist = false;
                                        //check if it exist in handlelist
                                        foreach (var handle in handles.GetAll()) {
                                            if (fileName.ToLower() == handle.FileName.ToLower()) {
                                                isExist = true;
                                                break;
                                            }
                                        }
                                        if (isExist) {
                                            result = ResponseCode.HandleSourceNeedOpen;
                                        }
                                        else {
                                            File.Delete(fileName);

                                            FileSystem.Flush(storage.Controller.Hdc);

                                            result = ResponseCode.Success;
                                        }
                                        break;
                                    }
                                    else {
                                        result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                    }

                                }
                            }
                            if (string.IsNullOrEmpty(result))
                                result = ResponseCode.MediaNotInitialize;

                            Bus.WriteLine(result);
                        }
                        catch {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    break;
                case CommandTypes.DeleteFolder:
                    if (cmd.ParamLength > 0) {
                        try {
                            var folderName = cmd.Parameters[0];

                            for (var i = 0; i < storages.Size; i++) {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null) {
                                    folderName = this.CurrentPath + folderName;
                                    if (Directory.Exists(folderName)) {
                                        var isExist = false;
                                        //check if it exist in handlelist
                                        foreach (var handle in handles.GetAll()) {
                                            if (folderName.ToLower() == handle.FileName.ToLower()) {
                                                isExist = true;
                                                break;
                                            }
                                        }
                                        if (isExist) {
                                            result = ResponseCode.HandleSourceNeedOpen;
                                        }
                                        else {
                                            Directory.Delete(folderName);

                                            FileSystem.Flush(storage.Controller.Hdc);

                                            result = ResponseCode.Success;
                                        }
                                        break;
                                    }
                                    else {
                                        result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                    }

                                }
                            }
                            if (string.IsNullOrEmpty(result))
                                result = ResponseCode.MediaNotInitialize;

                            Bus.WriteLine(result);
                        }
                        catch {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    break;
                case CommandTypes.FileListing:

                    try {
                        var isFound = false;
                        for (var i = 0; i < storages.Size; i++) {
                            var storage = storages.GetStorageByIndex(i);
                            if (storage != null) {
                                var dir1 = new DirectoryInfo(this.CurrentPath);
                                if (dir1.Exists) {
                                    isFound = true;
                                    fileExplorer.CurrentDirectory = this.CurrentPath;
                                    fileExplorer.Mode = FileExplorer.ExploreMode.Listing;
                                    fileExplorer.CurrentIndex = 0;
                                    fileExplorer.Clear();
                                    foreach (var file in dir1.GetFiles()) {
                                        fileExplorer.AddFile(file);
                                    }
                                    foreach (var dir in dir1.GetDirectories()) {
                                        fileExplorer.AddDirectory(dir);
                                    }
                                    result = ResponseCode.Success;
                                }
                                break;
                            }
                        }
                        if (!isFound)
                            Bus.Write(ResponseCode.MediaNotInitialize);
                        else
                            Bus.WriteLine(result);
                    }
                    catch {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.NextResult:
                    if (fileExplorer.Mode != FileExplorer.ExploreMode.Idle) {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        if (fileExplorer.CurrentIndex < fileExplorer.Count) {
                            var item = fileExplorer.GetByIndex(fileExplorer.CurrentIndex);
                            if (item != null) {
                                //attributes
                                var attrs = Strings.LeadingZero(string.Format("{0:X}", item.Attribute), 2);

                                //size
                                var size = Strings.LeadingZero(string.Format("{0:X}", item.Size), 8);

                                result += $"{item.Name} {attrs} {size}{Strings.NewLine}";

                                fileExplorer.CurrentIndex++;
                                result += ResponseCode.Success;
                            }
                            else {
                                result = ResponseCode.ERROR_ENDOF_FILEFOLDER_LIST;
                            }
                        }
                        else {
                            result = ResponseCode.ERROR_ENDOF_FILEFOLDER_LIST;
                        }
                        Bus.WriteLine(result);
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.NextResult2:

                    if (fileExplorer.Mode != FileExplorer.ExploreMode.Idle) {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        if (fileExplorer.CurrentIndex < fileExplorer.Count) {
                            var item = fileExplorer.GetByIndex(fileExplorer.CurrentIndex);
                            if (item != null) {
                                //attributes
                                var attrs = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Attribute), 2);

                                //size
                                var size = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Size), 8);

                                //name length
                                var namebytes = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Name.Length), 4);

                                result += $"{attrs} {size} {namebytes}{Strings.NewLine}";
                                var typeName = string.Empty;
                                if (cmd.ParamLength > 0) {
                                    typeName = cmd.Parameters[0].Trim();
                                }
                                if (typeName == "A") //ASCII
                                {
                                    //byte[] bytes = Encoding.UTF8.GetBytes(item.Name); //should be ascii
                                    //int res = BitConverter.ToInt32(bytes, 0);
                                    //result += $"{bytes}{Strings.NewLine}";
                                    result += $"{item.Name}{Strings.NewLine}";
                                }
                                else //unicode
                                {
                                    result += $"{item.Name}{Strings.NewLine}";
                                }
                                fileExplorer.CurrentIndex++;
                                result += ResponseCode.Success;
                            }
                            else {
                                result = ResponseCode.ERROR_ENDOF_FILEFOLDER_LIST;
                            }
                        }
                        else {
                            result = ResponseCode.ERROR_ENDOF_FILEFOLDER_LIST;
                        }
                        Bus.WriteLine(result);
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }

                    break;
                case CommandTypes.GetDateTime:
                    if (cmd.ParamLength > 0) {
                        var request = cmd.Parameters[0].Trim()[0];
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        switch (request) {
                            case 'X':
                                var tmp = ExFatTimeStampConverter.ConvertToFatTime(DateTime.Now);
                                result += $"${tmp}{Strings.NewLine}";
                                break;
                            case 'F':
                                result += $"{DateTime.Now.ToString("MM/dd/yyyy - hh:mm:ss")}{Strings.NewLine}";

                                break;
                        }
                        result += ResponseCode.Success;
                        Bus.WriteLine(result);

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.SetDateTime:
                    if (cmd.ParamLength > 0) {
                        var newdatestr = cmd.Parameters[0].Trim();
                        if (newdatestr.Length == 8) {
                            //conversion 
                            var newdate = ExFatTimeStampConverter.ConvertToDatetime(newdatestr);
                            if (this.TimerMode == TimerModes.Backup) {
                                var rtc = RtcController.GetDefault();
                                Console.WriteLine($"rtc status : { (rtc.IsValid ? "valid" : "not valid") }");
                                rtc.Now = newdate;
                            }
                            SystemTime.SetTime(newdate);
                            Bus.WriteLine(ResponseCode.Success);
                        }
                        else {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.InitTimer:
                    if (cmd.ParamLength > 0) {
                        var mode = cmd.Parameters[0].Trim()[0];
                        this.TimerMode = mode == 'S' ? TimerModes.Shared : TimerModes.Backup;
                        Bus.WriteLine(ResponseCode.Success);

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;

                case CommandTypes.FindFile:
                    if (cmd.ParamLength > 0) {
                        try {
                            var itemName = cmd.Parameters[0];

                            for (var i = 0; i < storages.Size; i++) {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null) {
                                    itemName = this.CurrentPath + itemName;
                                    //check if File 
                                    if (File.Exists(itemName)) {
                                        var file = new FileInfo(itemName);
                                        result += $"{ResponseCode.Success}{Strings.NewLine}";
                                        var lengths = "$" + Strings.LeadingZero(string.Format("{0:X}", file.Length), 8);
                                        var attrs = "$" + ((int)(file.Attributes)).ToString("x2");
                                        var dates = "$" + ExFatTimeStampConverter.ConvertToFatTime(file.LastWriteTime);
                                        result += $"{lengths} {attrs} {dates}{Strings.NewLine}";

                                        result += ResponseCode.Success;
                                        break;
                                    }
                                    else if (Directory.Exists(itemName)) //check if Directory
                                    {
                                        var dir = new DirectoryInfo(itemName);
                                        result += $"{ResponseCode.Success}{Strings.NewLine}";

                                        var lengths = "$" + Strings.LeadingZero(string.Format("{0:X}", 0), 8);
                                        var attrs = "$" + ((int)(dir.Attributes)).ToString("x2");
                                        var dates = "$" + ExFatTimeStampConverter.ConvertToFatTime(dir.LastWriteTime);
                                        result += $"{lengths} {attrs} {dates}{Strings.NewLine}";
                                        result += ResponseCode.Success;
                                        break;
                                    }
                                    else {
                                        result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                    }
                                }

                            }
                            Bus.WriteLine(result);
                        }
                        catch {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }

                    break;
                case CommandTypes.ChangeDirectory:
                    if (cmd.ParamLength > 0) {
                        var dirName = cmd.Parameters[0].Trim();
                        for (var i = 0; i < storages.Size; i++) {
                            var storage = storages.GetStorageByIndex(i);
                            if (storage != null) {
                                var newPath = this.CurrentPath + dirName;
                                if (Directory.Exists(newPath)) {
                                    this.CurrentPath = newPath + "\\";
                                    result = ResponseCode.Success;
                                }
                                else {
                                    result = ResponseCode.ERROR_FS_FILEFOLDER_NOT_EXIST;
                                }
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(result)) result = ResponseCode.MediaNotInitialize;
                        Bus.WriteLine(result);
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.GetMediaStatistic: {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        if (storages.Size > 0) {

                            for (var i = 0; i < storages.Size; i++) {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null) {
                                    var info = new DriveInfo(storage.Drive.Name);
                                    //size in sectors, divided by 512
                                    var mediaBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", info.TotalSize / 512), 8);
                                    var freeBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", info.TotalFreeSpace / 512), 8);
                                    result += $"{mediaBytes} {freeBytes}{Strings.NewLine}";
                                    break;
                                }
                            }
                        }
                        else {
                            var totalSize = 0;
                            var totalFreeSpace = 0;

                            var mediaBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", totalSize / 512), 8);
                            var freeBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", totalFreeSpace / 512), 8);
                            result += $"{mediaBytes} {freeBytes}{Strings.NewLine}";
                        }

                        result += ResponseCode.Success;
                        Bus.WriteLine(result);
                    }

                    break;
                case CommandTypes.MakeDirectory:
                    if (cmd.ParamLength > 0) {
                        try {
                            var dirName = cmd.Parameters[0].Trim();
                            for (var i = 0; i < storages.Size; i++) {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null) {
                                    var newPath = this.CurrentPath + dirName;
                                    if (!Directory.Exists(newPath)) {
                                        Directory.CreateDirectory(newPath);
                                        result = ResponseCode.Success;
                                    }
                                    else {
                                        result = ResponseCode.FileFolderExists;
                                    }
                                    break;
                                }
                            }
                            if (string.IsNullOrEmpty(result)) result = ResponseCode.MediaNotInitialize;
                        }
                        catch (Exception ex) {
                            result = ResponseCode.FailToWrite;
                        }

                        Bus.WriteLine(result);
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }

                    break;
                case CommandTypes.GetVersionNo:
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    result = $"{VersionNumber}{Strings.NewLine}";
                    result += ResponseCode.Success;
                    Bus.WriteLine(result);

                    break;
                case CommandTypes.SetBaudRate:
                    if (cmd.ParamLength > 0) {
                        var key = cmd.Parameters[0].Trim();
                        var newBaudRate = 0;
                        switch (this.PowerMode) {
                            case PowerModes.Full:
                                if (BaudRates.FullPower.Contains(key)) {
                                    newBaudRate = (int)BaudRates.FullPower[key];
                                }
                                break;
                            case PowerModes.Reduced:
                                if (BaudRates.ReducedPower.Contains(key)) {
                                    newBaudRate = (int)BaudRates.ReducedPower[key];
                                }
                                break;
                            default:
                                //do nothing
                                break;
                        }
                        if (newBaudRate > 0) {
                            //send success code and delay before set the new baud rate
                            Bus.WriteLine(ResponseCode.Success);
                            Thread.Sleep(100);
                            Bus.SetBaudRate(newBaudRate);
                            Bus.WriteLine(ResponseCode.Success);
                        }
                        else {
                            Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                        }

                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.FormatDrive:
                    if (cmd.ParamLength == 3) {
                        var media = Strings.Replace(cmd.Parameters[2].Trim(), ":", string.Empty);
                        var storage = storages.GetStorage(media);
                        if (storage == null) {
                            result = ResponseCode.MediaNotInitialize;
                        }
                        else {
                            //code for format drive here...
                            result = ResponseCode.Success;
                        }
                        Bus.WriteLine(result);
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.SetEcho:
                    if (cmd.ParamLength > 0) {
                        var echoVal = int.Parse(cmd.Parameters[0].Trim());
                        IsEchoEnabled = echoVal == 1;
                        Bus.WriteLine(ResponseCode.Success);
                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                case CommandTypes.SetPowerMode:
                    if (cmd.ParamLength > 0) {
                        var mode = cmd.Parameters[0][0];
                        var newBaudRate = Convert.ToInt32("0x" + cmd.Parameters[0].Substring(1, cmd.Parameters[0].Length - 1).Trim(), 16);
                        Bus.WriteLine(ResponseCode.Success);
                        this.PowerMode = (PowerModes)mode;
                        switch (this.PowerMode) {
                            case PowerModes.Full:
                                //do something
                                Bus.WriteLine(ResponseCode.Success);
                                break;
                            case PowerModes.Reduced:
                                //do something
                                Bus.WriteLine(ResponseCode.Success);
                                break;
                            case PowerModes.Hibernate:
                                //do something
                                var ldrButton = GpioController.GetDefault().OpenPin(this.LDRPin);
                                ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
                                ldrButton.ValueChanged += (GpioPin sender, GpioPinValueChangedEventArgs e) => { };
                                Bus.WriteLine(ResponseCode.Success);
                                //The next line starts Sleep.
                                Power.Sleep();
                                break;
                        }


                    }
                    else {
                        Bus.WriteLine(ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    }
                    break;
                default:
                    Bus.WriteLine(string.IsNullOrEmpty(data) ? ResponseCode.Success : ResponseCode.ERROR_COMMANDER_INCORRECT_CMD_PARAMETER);
                    break;

            }
        }

        public bool ConnectSD(string mediaName) {
            if (!this.IsSDConnected) {
                try {

                    var sd = StorageController.FromName(this.SDControllerName);
                    var drive = FileSystem.Mount(sd.Hdc);
                    var driveInfo = new DriveInfo(drive.Name);
                    if (string.IsNullOrEmpty(this.CurrentPath))
                        this.CurrentPath = driveInfo.RootDirectory.FullName;
                    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = sd, Drive = drive, Name = mediaName });
                    this.IsSDConnected = true;
                }
                catch (Exception) {
                    this.IsSDConnected = false;
                    //throw;
                }
            }
            return this.IsSDConnected;
        }
        void InitUsbHost() {
            if (this.IsUsbHostInitialized)
                return;

            Thread.Sleep(1);

            if (UsbHost == null) {
                UsbHost = GHIElectronics.TinyCLR.Devices.UsbHost.
                UsbHostController.GetDefault();

                UsbHost.OnConnectionChangedEvent +=
                    this.UsbHostController_OnConnectionChangedEvent;

                UsbHost.Enable();

                this.IsUsbHostInitialized = true;

                var timeout = DateTime.Now;

                while (this.IsKeyboardConnected == false &&
                    this.IsUsbDiskConnected == false
                    ) {
                    var duration = DateTime.Now - timeout;


                    if (duration.TotalMilliseconds > UsbHostConnectionTimeoutMillisecond)
                        break;

                    Thread.Sleep(1);
                }
            }
        }
        private void UsbHostController_OnConnectionChangedEvent
       (GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController sender,
       GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionEventArgs e) {

            //System.Diagnostics.Debug.WriteLine("e.Id = " + e.Id + " \n");
            //System.Diagnostics.Debug.WriteLine("e.InterfaceIndex = " + e.InterfaceIndex + " \n");
            //System.Diagnostics.Debug.WriteLine("e.PortNumber = " + e.PortNumber);
            //System.Diagnostics.Debug.WriteLine("e.Type = " + ((object)(e.Type)).
            //    ToString() + " \n");

            //System.Diagnostics.Debug.WriteLine("e.VendorId = " + e.VendorId + " \n");
            //System.Diagnostics.Debug.WriteLine("e.ProductId = " + e.ProductId + " \n");

            switch (e.DeviceStatus) {
                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Connected:
                    switch (e.Type) {
                        case GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice.
                            DeviceType.Keyboard:

                            var keyboard = new GHIElectronics.TinyCLR.Devices.UsbHost.
                                Keyboard(e.Id, e.InterfaceIndex);

                            keyboard.KeyUp += Keyboard_KeyUp;
                            keyboard.KeyDown += Keyboard_KeyDown;
                            this.IsKeyboardConnected = true;
                            this.IsSDConnected = false;
                            this.IsUsbDiskConnected = false;
                            break;

                        case GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice.DeviceType.Mouse:
                            //do nothing

                            break;

                        case GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice.
                            DeviceType.MassStorage:

                            //var storageController = StorageController.FromName(this.StorageControllerName);
                            //IDriveProvider driver;
                            //try {
                            //    driver = GHIElectronics.TinyCLR.IO.FileSystem.
                            //        Mount(storageController.Hdc);
                            //}
                            //catch (Exception ex) {
                            //    //ummount -> then mount
                            //    FileSystem.Unmount(storageController.Hdc);
                            //    driver = GHIElectronics.TinyCLR.IO.FileSystem.
                            //      Mount(storageController.Hdc);
                            //}
                            //if (driver != null) {
                            //    var driveInfo = new System.IO.DriveInfo(driver.Name);
                            //    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U0 });
                            //    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U1 });
                            //    if (string.IsNullOrEmpty(this.CurrentPath))
                            //        this.CurrentPath = driveInfo.RootDirectory.FullName;
                            //    System.Diagnostics.Debug.WriteLine
                            //        ("Free: " + driveInfo.TotalFreeSpace);

                            //    System.Diagnostics.Debug.WriteLine
                            //        ("TotalSize: " + driveInfo.TotalSize);

                            //    System.Diagnostics.Debug.WriteLine
                            //        ("VolumeLabel:" + driveInfo.VolumeLabel);

                            //    System.Diagnostics.Debug.WriteLine
                            //        ("RootDirectory: " + driveInfo.RootDirectory);

                            //    System.Diagnostics.Debug.WriteLine
                            //        ("DriveFormat: " + driveInfo.DriveFormat);
                            //}
                            this.IsKeyboardConnected = false;
                            this.IsSDConnected = false;
                            this.IsUsbDiskConnected = true;
                            break;

                        default:
                            this.IsKeyboardConnected = false;
                            this.IsSDConnected = false;
                            this.IsUsbDiskConnected = false;
                            //do nothing

                            break;
                    }
                    break;

                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Disconnected:
                    //System.Diagnostics.Debug.WriteLine("Device Disconnected");
                    //unmount if there is usb disk connected
                    if (this.IsUsbDiskConnected && this.IsUsbDiskInitialized) {
                        var storageController = StorageController.FromName(this.StorageControllerName);
                        FileSystem.Unmount(storageController.Hdc);
                        //remove from list
                        storages.RemoveStorage(MediaTypes.U0);
                        storages.RemoveStorage(MediaTypes.U1);
                    }
                    this.IsKeyboardConnected = false;
                    this.IsSDConnected = false;
                    this.IsUsbDiskConnected = false;
                    this.IsUsbDiskInitialized = false;
                    //Unmount filesystem if it was mounted.
                    break;

                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Bad:
                    //System.Diagnostics.Debug.WriteLine("Bad Device");
                    this.IsKeyboardConnected = false;
                    this.IsSDConnected = false;
                    this.IsUsbDiskConnected = false;
                    break;
            }
        }

        private static void Keyboard_KeyDown(GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard.KeyboardEventArgs args) {

            //System.Diagnostics.Debug.WriteLine("Key pressed: " + ((object)args.Which).ToString());
            //System.Diagnostics.Debug.WriteLine("Key pressed ASCII: " +
            //    ((object)args.ASCII).ToString());
        }

        private static void Keyboard_KeyUp(GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard.KeyboardEventArgs args) {

            //System.Diagnostics.Debug.WriteLine
            //    ("Key released: " + ((object)args.Which).ToString());

            //System.Diagnostics.Debug.WriteLine
            //    ("Key released ASCII: " + ((object)args.ASCII).ToString());
        }

        //private static void Mouse_CursorMoved(GHIElectronics.TinyCLR.Devices.UsbHost.Mouse
        //    sender, GHIElectronics.TinyCLR.Devices.UsbHost.Mouse.CursorMovedEventArgs e) => System.Diagnostics.Debug.WriteLine("Mouse moved to: " + e.NewPosition.X +
        //         ", " + e.NewPosition.Y);

        //private static void Mouse_ButtonChanged(GHIElectronics.TinyCLR.Devices.UsbHost.Mouse
        //    sender, GHIElectronics.TinyCLR.Devices.UsbHost.Mouse.ButtonChangedEventArgs args) => System.Diagnostics.Debug.WriteLine
        //        ("Mouse button changed: " + ((object)args.Which).ToString());


    }
}
