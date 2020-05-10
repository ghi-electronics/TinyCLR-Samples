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

namespace uAlfat.Core
{
    public class uAlfatModule
    {
        public enum PowerModes
        {
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
        public bool IsSDConnected { get; set; }
        public bool IsKeyboardConnected { get; set; }
        string StorageControllerName { get; set; }
        public static GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController UsbHost
        { set; get; }
        public static CommunicationsBus Bus { get; set; }
        public uAlfatModule(string uartPort, string storageControllerName, string sDControllerName, int ldrPin = SC20260.GpioPin.PE3)
        {
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
            Bus.DataReceived += this.ProcessCommand;
            this.StorageControllerName = storageControllerName;
            this.SDControllerName = sDControllerName;
            this.InitUsbHost();
            Console.WriteLine("uAlfat is ready");
        }

        private void ProcessCommand(string data)
        {
            var isSuccess = false;
            var result = string.Empty;

            var cmd = CommandParser.Parse(data);
            string[] par1;
            //alfat commands
            switch (cmd.CommandPrefix)
            {
                case CommandTypes.Init:
                    {
                        //try connect sd card
                        isSuccess = this.ConnectSD(MediaTypes.D);
                        if (isSuccess)
                        {
                            result = ResponseCode.Success;
                        }
                        else
                        {
                            result = ResponseCode.NoSDCard;
                        }
                        if (!isSuccess)
                        {
                            //try connect usb disk
                            if (this.IsUsbDiskConnected)
                            {
                                isSuccess = true;
                                result = ResponseCode.Success;
                            }
                            else
                            {
                                result = ResponseCode.NoSDCard;
                            }
                        }
                        if (isSuccess)
                            Bus.WriteLine(ResponseCode.Success);
                        else
                            Bus.WriteLine(ResponseCode.NoSDCard);
                    }
                    break;
                case CommandTypes.MountUsb:
                    {
                        //try connect usb disk
                        if (this.IsUsbDiskConnected)
                        {
                            isSuccess = true;
                            result = ResponseCode.Success;
                        }
                        else
                        {
                            result = ResponseCode.NoSDCard;
                        }

                        if (isSuccess)
                            Bus.WriteLine(ResponseCode.Success);
                        else
                            Bus.WriteLine(ResponseCode.NoSDCard);
                    }
                    break;
                case CommandTypes.DetectUsb:
                    {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        //try connect usb disk
                        if (this.IsUsbDiskConnected)
                        {
                            isSuccess = true;
                            result += "$01" + Strings.NewLine;
                        }
                        else
                        {
                            result += "$00" + Strings.NewLine;
                        }
                        result += ResponseCode.Success;

                        Bus.WriteLine(result);
                    }
                    break;
                case CommandTypes.Open:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Trim().Split('>');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var fileName = par1[1];
                                var handle = par1[0][0];
                                var accessType = par1[0][1];

                                //is handle available
                                if (handles.IsExist(handle))
                                {
                                    result = ResponseCode.HandleAlreadyUsed;
                                }
                                else
                                {
                                    //has storage been init ?
                                    for (var i = 0; i < storages.Size; i++)
                                    {
                                        var storage = storages.GetStorageByIndex(i);
                                        if (storage != null)
                                        {
                                            fileName = $"{this.CurrentPath}{fileName}";
                                            var newHandle = new MediaHandle() { AccessType = accessType, HandleName = handle, FileName = fileName, Media = storage.Name };

                                            switch (accessType)
                                            {
                                                case FileAccessTypes.Read:

                                                    if (File.Exists(fileName))
                                                    {
                                                        var contentBytes = File.ReadAllBytes(fileName);
                                                        newHandle.Buffer = new MemoryStream(contentBytes);
                                                        newHandle.Size = contentBytes.Length;
                                                        result = ResponseCode.Success;
                                                    }
                                                    else
                                                    {
                                                        result = ResponseCode.FileFolderNotExist;
                                                    }
                                                    break;
                                                case FileAccessTypes.Write:

                                                    if (File.Exists(fileName))
                                                    {
                                                        result = ResponseCode.FileFolderExists;
                                                    }
                                                    else
                                                    {
                                                        result = ResponseCode.Success;
                                                    }
                                                    break;
                                                case FileAccessTypes.Append:

                                                    if (File.Exists(fileName))
                                                    {
                                                        var contentBytes = File.ReadAllBytes(fileName);
                                                        newHandle.Buffer = new MemoryStream(contentBytes);
                                                        //set to end of file
                                                        newHandle.CursorPosition = contentBytes.Length - 1;
                                                        result = ResponseCode.Success;
                                                    }
                                                    else
                                                    {
                                                        result = ResponseCode.FileFolderNotExist;
                                                    }
                                                    break;
                                            }
                                            if (result == ResponseCode.Success)
                                            {
                                                handles.AddHandle(newHandle);
                                            }

                                            break;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(result)) result = ResponseCode.MediaNotInitialize;


                                }
                            }
                            catch
                            {
                                result = ResponseCode.IncorrectParameter;
                            }

                            Bus.WriteLine(result);
                        }
                        else
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.Close:
                    if (cmd.ParamLength > 0)
                    {
                        var handle = cmd.Parameters[0].Trim()[0];

                        if (handles.IsExist(handle))
                        {
                            //if write/append mode then flush
                            var currentHandle = handles.GetHandle(handle);
                            if (currentHandle.AccessType == FileAccessTypes.Write || currentHandle.AccessType == FileAccessTypes.Append)
                            {
                                var byteToWrite = new byte[currentHandle.CursorPosition];
                                currentHandle.Buffer.Seek(0, SeekOrigin.Begin);
                                for (var i = 0; i < byteToWrite.Length; i++)
                                {
                                    byteToWrite[i] = (byte)currentHandle.Buffer.ReadByte();
                                }

                                //flush memory to file
                                File.WriteAllBytes(currentHandle.FileName, byteToWrite);

                            }
                            //make it available
                            currentHandle.Buffer.Dispose();
                            var res = handles.RemoveHandle(handle);
                            result = res ? ResponseCode.Success : ResponseCode.InvalidHandle;
                        }
                        else
                        {
                            result = ResponseCode.InvalidHandle;
                        }
                        Bus.WriteLine(result);

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.Tell:
                    if (cmd.ParamLength > 0)
                    {
                        var handle = cmd.Parameters[0].Trim()[0];
                        var currentHandle = handles.GetHandle(handle);
                        if (currentHandle != null)
                        {
                            result = $"{ResponseCode.Success}{Strings.NewLine}";
                            //write current read position
                            var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", currentHandle.CursorPosition), 8);
                            result += $"{actBytes}{Strings.NewLine}";
                            result += ResponseCode.Success;
                        }
                        else
                        {
                            result = ResponseCode.InvalidHandle;
                        }
                        Bus.WriteLine(result);

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.Read:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var numReadBytes = Convert.ToInt32("0x" + par1[1], 16);
                                var handle = par1[0][0];
                                var pad = par1[0][1];
                                var currentHandle = handles.GetHandle(handle);
                                //is handle available
                                if (currentHandle == null)
                                {
                                    result = ResponseCode.InvalidHandle;
                                }
                                else if (currentHandle.AccessType != FileAccessTypes.Read)
                                {
                                    result = ResponseCode.HandleRequireRead;
                                }
                                else
                                {

                                    //has storage been init ?
                                    var storage = storages.GetStorage(currentHandle.Media);
                                    if (storage == null)
                                    {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else
                                    {
                                        if (currentHandle.Buffer.Length > 0)
                                        {
                                            result = ResponseCode.Success + Strings.NewLine;
                                            if (currentHandle.Buffer.Length < numReadBytes + currentHandle.CursorPosition)
                                            {
                                                var lengthRead = currentHandle.Buffer.Length - currentHandle.CursorPosition; //numReadBytes - currentHandle.Buffer.Length;
                                                var readBytes = new byte[lengthRead];
                                                currentHandle.Buffer.Seek(currentHandle.CursorPosition, SeekOrigin.Begin);
                                                for (var i = 0; i < lengthRead; i++)
                                                {
                                                    readBytes[i] = (byte)currentHandle.Buffer.ReadByte();
                                                }
                                                var numberOfPad = (numReadBytes + currentHandle.CursorPosition) - currentHandle.Buffer.Length;
                                                //go to beginning of file
                                                currentHandle.CursorPosition = 0;
                                                var contentStr = Encoding.UTF8.GetString(readBytes);
                                                //read data size
                                                var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", contentStr.Length), 8);
                                                //content
                                                contentStr += Strings.GetFiller(pad, numberOfPad);
                                                result += $"{contentStr}{actBytes}{Strings.NewLine}";
                                            }
                                            else
                                            {
                                                var readBytes = new byte[numReadBytes];
                                                currentHandle.Buffer.Seek(currentHandle.CursorPosition, SeekOrigin.Begin);
                                                for (var i = 0; i < numReadBytes; i++)
                                                {
                                                    readBytes[i] = (byte)currentHandle.Buffer.ReadByte();
                                                }
                                                currentHandle.CursorPosition += numReadBytes;
                                                //reset to beginning of file
                                                if (currentHandle.CursorPosition >= currentHandle.Buffer.Length)
                                                    currentHandle.CursorPosition = 0;
                                                //content
                                                var contentStr = Encoding.UTF8.GetString(readBytes);
                                                //read size
                                                var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", numReadBytes), 8);

                                                result += $"{contentStr}{actBytes}{Strings.NewLine}";

                                            }
                                            result += ResponseCode.Success;
                                        }
                                        else
                                        {
                                            result = ResponseCode.FileFolderNotExist;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                result = ResponseCode.IncorrectParameter;
                            }

                            Bus.WriteLine(result);
                        }
                        else
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.Write:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var handle = par1[0][0];
                                var numWriteBytes = Convert.ToInt32("0x" + par1[1], 16);

                                var currentHandle = handles.GetHandle(handle);
                                //is handle available
                                if (currentHandle == null)
                                {
                                    result = ResponseCode.InvalidHandle;
                                }
                                else if (currentHandle.AccessType != FileAccessTypes.Write && currentHandle.AccessType != FileAccessTypes.Append)
                                {
                                    result = ResponseCode.HandleRequireAppend;
                                }
                                else
                                {
                                    //has storage been init ?
                                    var storage = storages.GetStorage(currentHandle.Media);
                                    if (storage == null)
                                    {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else
                                    {
                                        var readDataBytes = new byte[numWriteBytes];
                                        //read data to be written
                                        if (cmd.NextLine != string.Empty)
                                        {
                                            readDataBytes = UTF8Encoding.UTF8.GetBytes(cmd.NextLine);
                                        }
                                        else
                                        {
                                            Bus.Read(readDataBytes);
                                        }

                                        for (var i = 0; i < readDataBytes.Length; i++)
                                        {
                                            currentHandle.Buffer.WriteByte(readDataBytes[i]);
                                        }
                                        currentHandle.CursorPosition += readDataBytes.Length;
                                        currentHandle.Size = currentHandle.CursorPosition;
                                        var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", readDataBytes.Length), 8);

                                        result += actBytes + Strings.NewLine;
                                        result += ResponseCode.Success;
                                    }
                                }
                            }
                            catch
                            {
                                result = ResponseCode.IncorrectParameter;
                            }

                            Bus.WriteLine(result);
                        }
                        else
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.Seek:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var handle = par1[0][0];
                                long newPosition = Convert.ToInt32("0x" + par1[1], 16);

                                var currentHandle = handles.GetHandle(handle);
                                //is handle available
                                if (currentHandle == null)
                                {
                                    result = ResponseCode.InvalidHandle;
                                }
                                else if (currentHandle.AccessType != FileAccessTypes.Read)
                                {
                                    result = ResponseCode.HandleRequireRead;
                                }
                                else
                                {
                                    //has storage been init ?
                                    var storage = storages.GetStorage(currentHandle.Media);
                                    if (storage == null)
                                    {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else
                                    {
                                        //if new position > file size, go to EOF
                                        if (newPosition > currentHandle.Size)
                                            newPosition = currentHandle.Size;

                                        currentHandle.CursorPosition = newPosition;

                                        result = ResponseCode.Success;
                                    }
                                }
                            }
                            catch
                            {
                                result = ResponseCode.IncorrectParameter;
                            }

                            Bus.WriteLine(result);
                        }
                        else
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.Delete:
                    if (cmd.ParamLength > 0)
                    {
                        try
                        {
                            var fileName = cmd.Parameters[0];

                            for (var i = 0; i < storages.Size; i++)
                            {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null)
                                {
                                    fileName = this.CurrentPath + fileName;
                                    if (File.Exists(fileName))
                                    {
                                        var isExist = false;
                                        //check if it exist in handlelist
                                        foreach (var handle in handles.GetAll())
                                        {
                                            if (fileName.ToLower() == handle.FileName.ToLower())
                                            {
                                                isExist = true;
                                                break;
                                            }
                                        }
                                        if (isExist)
                                        {
                                            result = ResponseCode.HandleSourceNeedOpen;
                                        }
                                        else
                                        {
                                            File.Delete(fileName);
                                            result = ResponseCode.Success;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
                                    }

                                }
                            }
                            if (string.IsNullOrEmpty(result))
                                result = ResponseCode.MediaNotInitialize;

                            Bus.WriteLine(result);
                        }
                        catch
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    break;
                case CommandTypes.DeleteFolder:
                    if (cmd.ParamLength > 0)
                    {
                        try
                        {
                            var folderName = cmd.Parameters[0];

                            for (var i = 0; i < storages.Size; i++)
                            {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null)
                                {
                                    folderName = this.CurrentPath + folderName;
                                    if (Directory.Exists(folderName))
                                    {
                                        var isExist = false;
                                        //check if it exist in handlelist
                                        foreach (var handle in handles.GetAll())
                                        {
                                            if (folderName.ToLower() == handle.FileName.ToLower())
                                            {
                                                isExist = true;
                                                break;
                                            }
                                        }
                                        if (isExist)
                                        {
                                            result = ResponseCode.HandleSourceNeedOpen;
                                        }
                                        else
                                        {
                                            Directory.Delete(folderName);
                                            result = ResponseCode.Success;
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
                                    }

                                }
                            }
                            if (string.IsNullOrEmpty(result))
                                result = ResponseCode.MediaNotInitialize;

                            Bus.WriteLine(result);
                        }
                        catch
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    break;
                case CommandTypes.FileListing:

                    try
                    {
                        var isFound = false;
                        for (var i = 0; i < storages.Size; i++)
                        {
                            var storage = storages.GetStorageByIndex(i);
                            if (storage != null)
                            {
                                var dir1 = new DirectoryInfo(this.CurrentPath);
                                if (dir1.Exists)
                                {
                                    isFound = true;
                                    fileExplorer.CurrentDirectory = this.CurrentPath;
                                    fileExplorer.Mode = FileExplorer.ExploreMode.Listing;
                                    fileExplorer.CurrentIndex = 0;
                                    fileExplorer.Clear();
                                    foreach (var file in dir1.GetFiles())
                                    {
                                        fileExplorer.AddFile(file);
                                    }
                                    foreach (var dir in dir1.GetDirectories())
                                    {
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
                    catch
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.NextResult:
                    if (fileExplorer.Mode != FileExplorer.ExploreMode.Idle)
                    {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        if (fileExplorer.CurrentIndex < fileExplorer.Count)
                        {
                            var item = fileExplorer.GetByIndex(fileExplorer.CurrentIndex);
                            if (item != null)
                            {
                                //attributes
                                var attrs = Strings.LeadingZero(string.Format("{0:X}", item.Attribute), 2);

                                //size
                                var size = Strings.LeadingZero(string.Format("{0:X}", item.Size), 8);

                                result += $"{item.Name} {attrs} {size}{Strings.NewLine}";

                                fileExplorer.CurrentIndex++;
                                result += ResponseCode.Success;
                            }
                            else
                            {
                                result = ResponseCode.EndOfFile;
                            }
                        }
                        else
                        {
                            result = ResponseCode.EndOfFile;
                        }
                        Bus.WriteLine(result);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.Unknown);
                    }
                    break;
                case CommandTypes.NextResult2:

                    if (fileExplorer.Mode != FileExplorer.ExploreMode.Idle)
                    {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        if (fileExplorer.CurrentIndex < fileExplorer.Count)
                        {
                            var item = fileExplorer.GetByIndex(fileExplorer.CurrentIndex);
                            if (item != null)
                            {
                                //attributes
                                var attrs = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Attribute), 2);

                                //size
                                var size = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Size), 8);

                                //name length
                                var namebytes = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Name.Length), 4);

                                result += $"{attrs} {size} {namebytes}{Strings.NewLine}";
                                var typeName = string.Empty;
                                if (cmd.ParamLength > 0)
                                {
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
                            else
                            {
                                result = ResponseCode.EndOfFile;
                            }
                        }
                        else
                        {
                            result = ResponseCode.EndOfFile;
                        }
                        Bus.WriteLine(result);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.Unknown);
                    }

                    break;
                case CommandTypes.GetDateTime:
                    if (cmd.ParamLength > 0)
                    {
                        var request = cmd.Parameters[0].Trim()[0];
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        switch (request)
                        {
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
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.SetDateTime:
                    if (cmd.ParamLength > 0)
                    {
                        var newdatestr = cmd.Parameters[0].Trim();
                        if (newdatestr.Length == 8)
                        {
                            //conversion 
                            var newdate = ExFatTimeStampConverter.ConvertToDatetime(newdatestr);
                            if (this.TimerMode == TimerModes.Backup)
                            {
                                var rtc = RtcController.GetDefault();
                                Console.WriteLine($"rtc status : { (rtc.IsValid ? "valid" : "not valid") }");
                                rtc.Now = newdate;
                            }
                            SystemTime.SetTime(newdate);
                            Bus.WriteLine(ResponseCode.Success);
                        }
                        else
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.InitTimer:
                    if (cmd.ParamLength > 0)
                    {
                        var mode = cmd.Parameters[0].Trim()[0];
                        this.TimerMode = mode == 'S' ? TimerModes.Shared : TimerModes.Backup;
                        Bus.WriteLine(ResponseCode.Success);

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;

                case CommandTypes.FindFile:
                    if (cmd.ParamLength > 0)
                    {
                        try
                        {
                            var itemName = cmd.Parameters[0];

                            for (var i = 0; i < storages.Size; i++)
                            {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null)
                                {
                                    itemName = this.CurrentPath + itemName;
                                    //check if File 
                                    if (File.Exists(itemName))
                                    {
                                        var file = new FileInfo(itemName);
                                        result += $"{ResponseCode.Success}{Strings.NewLine}";
                                        var lengths = "$" + Strings.LeadingZero(string.Format("{0:X}", file.Length), 8);
                                        var attrs = "$" + Strings.LeadingZero(string.Format("{0:X}", ItemResult.GetAttribute(file.Attributes)), 2);
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
                                        var attrs = "$" + Strings.LeadingZero(string.Format("{0:X}", ItemResult.GetAttribute(dir.Attributes)), 2);
                                        var dates = "$" + ExFatTimeStampConverter.ConvertToFatTime(dir.LastWriteTime);
                                        result += $"{lengths} {attrs} {dates}{Strings.NewLine}";
                                        result += ResponseCode.Success;
                                        break;
                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
                                    }
                                }

                            }
                            Bus.WriteLine(result);
                        }
                        catch
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }

                    break;
                case CommandTypes.ChangeDirectory:
                    if (cmd.ParamLength > 0)
                    {
                        var dirName = cmd.Parameters[0].Trim();
                        for (var i = 0; i < storages.Size; i++)
                        {
                            var storage = storages.GetStorageByIndex(i);
                            if (storage != null)
                            {
                                var newPath = this.CurrentPath + dirName;
                                if (Directory.Exists(newPath))
                                {
                                    this.CurrentPath = newPath + "\\";
                                    result = ResponseCode.Success;
                                }
                                else
                                {
                                    result = ResponseCode.FileFolderNotExist;
                                }
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(result)) result = ResponseCode.MediaNotInitialize;
                        Bus.WriteLine(result);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.GetMediaStatistic:

                    {
                        result = $"{ResponseCode.Success}{Strings.NewLine}";
                        for (var i = 0; i < storages.Size; i++)
                        {
                            var storage = storages.GetStorageByIndex(i);
                            if (storage != null)
                            {
                                var info = new DriveInfo(storage.Drive.Name);
                                var mediaBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", info.TotalSize), 8);
                                var freeBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", info.TotalFreeSpace), 8);
                                result += $"{mediaBytes} {freeBytes}{Strings.NewLine}";
                                break;
                            }
                        }
                        result += ResponseCode.Success;
                        Bus.WriteLine(result);
                    }

                    break;
                case CommandTypes.MakeDirectory:
                    if (cmd.ParamLength > 0)
                    {
                        try
                        {
                            var dirName = cmd.Parameters[0].Trim();
                            for (var i = 0; i < storages.Size; i++)
                            {
                                var storage = storages.GetStorageByIndex(i);
                                if (storage != null)
                                {
                                    var newPath = this.CurrentPath + dirName;
                                    if (!Directory.Exists(newPath))
                                    {
                                        Directory.CreateDirectory(newPath);
                                        result = ResponseCode.Success;
                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderExists;
                                    }
                                    break;
                                }
                            }
                            if (string.IsNullOrEmpty(result)) result = ResponseCode.MediaNotInitialize;
                        }
                        catch (Exception ex)
                        {
                            result = ResponseCode.FailToWrite;
                        }

                        Bus.WriteLine(result);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }

                    break;
                case CommandTypes.GetVersionNo:
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    result = $"V{version}{Strings.NewLine}";
                    result += ResponseCode.Success;
                    Bus.WriteLine(result);

                    break;
                case CommandTypes.SetBaudRate:
                    if (cmd.ParamLength > 0)
                    {
                        var key = cmd.Parameters[0].Trim();
                        var newBaudRate = 0;
                        switch (this.PowerMode)
                        {
                            case PowerModes.Full:
                                if (BaudRates.FullPower.Contains(key))
                                {
                                    newBaudRate = (int)BaudRates.FullPower[key];
                                }
                                break;
                            case PowerModes.Reduced:
                                if (BaudRates.ReducedPower.Contains(key))
                                {
                                    newBaudRate = (int)BaudRates.ReducedPower[key];
                                }
                                break;
                            default:
                                //do nothing
                                break;
                        }
                        if (newBaudRate > 0)
                        {
                            Bus.WriteLine(ResponseCode.Success);
                            Bus.SetBaudRate(newBaudRate);
                            Bus.WriteLine(ResponseCode.Success);
                        }
                        else
                        {
                            Bus.WriteLine(ResponseCode.IncorrectParameter);
                        }

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.FormatDrive:
                    if (cmd.ParamLength == 3)
                    {
                        var media = Strings.Replace(cmd.Parameters[2].Trim(), ":", string.Empty);
                        var storage = storages.GetStorage(media);
                        if (storage == null)
                        {
                            result = ResponseCode.MediaNotInitialize;
                        }
                        else
                        {
                            //code for format drive here...
                            result = ResponseCode.Success;
                        }
                        Bus.WriteLine(result);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.SetEcho:
                    if (cmd.ParamLength > 0)
                    {
                        var echoVal = int.Parse(cmd.Parameters[0].Trim());
                        IsEchoEnabled = echoVal == 1;
                        Bus.WriteLine(ResponseCode.Success);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.SetPowerMode:
                    if (cmd.ParamLength > 0)
                    {
                        var mode = cmd.Parameters[0][0];
                        var newBaudRate = Convert.ToInt32("0x" + cmd.Parameters[0].Substring(1, cmd.Parameters[0].Length - 1).Trim(), 16);
                        Bus.WriteLine(ResponseCode.Success);
                        this.PowerMode = (PowerModes)mode;
                        switch (this.PowerMode)
                        {
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
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                default:
                    Bus.WriteLine(string.IsNullOrEmpty(data) ? ResponseCode.Success : ResponseCode.Unknown);
                    break;

            }
            //echo
            if (IsEchoEnabled)
            {
                Bus.WriteLine(data);
            }
        }

        public bool ConnectSD(string mediaName)
        {
            if (!this.IsSDConnected)
            {
                try
                {

                    var sd = StorageController.FromName(this.SDControllerName);
                    var drive = FileSystem.Mount(sd.Hdc);
                    var driveInfo = new DriveInfo(drive.Name);
                    if (string.IsNullOrEmpty(this.CurrentPath))
                        this.CurrentPath = driveInfo.RootDirectory.FullName;
                    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = sd, Drive = drive, Name = mediaName });
                    this.IsSDConnected = true;
                }
                catch (Exception)
                {
                    this.IsSDConnected = false;
                    //throw;
                }
            }
            return this.IsSDConnected;
        }
        void InitUsbHost()
        {
            if (UsbHost == null)
            {
                UsbHost = GHIElectronics.TinyCLR.Devices.UsbHost.
                UsbHostController.GetDefault();

                UsbHost.OnConnectionChangedEvent +=
                    this.UsbHostController_OnConnectionChangedEvent;

                UsbHost.Enable();
            }
        }
        private void UsbHostController_OnConnectionChangedEvent
       (GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController sender,
       GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionEventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("e.Id = " + e.Id + " \n");
            System.Diagnostics.Debug.WriteLine("e.InterfaceIndex = " + e.InterfaceIndex + " \n");
            System.Diagnostics.Debug.WriteLine("e.PortNumber = " + e.PortNumber);
            System.Diagnostics.Debug.WriteLine("e.Type = " + ((object)(e.Type)).
                ToString() + " \n");

            System.Diagnostics.Debug.WriteLine("e.VendorId = " + e.VendorId + " \n");
            System.Diagnostics.Debug.WriteLine("e.ProductId = " + e.ProductId + " \n");

            switch (e.DeviceStatus)
            {
                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Connected:
                    switch (e.Type)
                    {
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

                            var storageController = StorageController.FromName(this.StorageControllerName);

                            var driver = GHIElectronics.TinyCLR.IO.FileSystem.
                                Mount(storageController.Hdc);

                            var driveInfo = new System.IO.DriveInfo(driver.Name);
                            storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U0 });
                            storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U1 });
                            if (string.IsNullOrEmpty(this.CurrentPath))
                                this.CurrentPath = driveInfo.RootDirectory.FullName;
                            System.Diagnostics.Debug.WriteLine
                                ("Free: " + driveInfo.TotalFreeSpace);

                            System.Diagnostics.Debug.WriteLine
                                ("TotalSize: " + driveInfo.TotalSize);

                            System.Diagnostics.Debug.WriteLine
                                ("VolumeLabel:" + driveInfo.VolumeLabel);

                            System.Diagnostics.Debug.WriteLine
                                ("RootDirectory: " + driveInfo.RootDirectory);

                            System.Diagnostics.Debug.WriteLine
                                ("DriveFormat: " + driveInfo.DriveFormat);
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
                    System.Diagnostics.Debug.WriteLine("Device Disconnected");
                    this.IsKeyboardConnected = false;
                    this.IsSDConnected = false;
                    this.IsUsbDiskConnected = false;
                    //Unmount filesystem if it was mounted.
                    break;

                case GHIElectronics.TinyCLR.Devices.UsbHost.DeviceConnectionStatus.Bad:
                    System.Diagnostics.Debug.WriteLine("Bad Device");
                    this.IsKeyboardConnected = false;
                    this.IsSDConnected = false;
                    this.IsUsbDiskConnected = false;
                    break;
            }
        }

        private static void Keyboard_KeyDown(GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard.KeyboardEventArgs args)
        {

            System.Diagnostics.Debug.WriteLine("Key pressed: " + ((object)args.Which).ToString());
            System.Diagnostics.Debug.WriteLine("Key pressed ASCII: " +
                ((object)args.ASCII).ToString());
        }

        private static void Keyboard_KeyUp(GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Keyboard.KeyboardEventArgs args)
        {

            System.Diagnostics.Debug.WriteLine
                ("Key released: " + ((object)args.Which).ToString());

            System.Diagnostics.Debug.WriteLine
                ("Key released ASCII: " + ((object)args.ASCII).ToString());
        }

        private static void Mouse_CursorMoved(GHIElectronics.TinyCLR.Devices.UsbHost.Mouse
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Mouse.CursorMovedEventArgs e) => System.Diagnostics.Debug.WriteLine("Mouse moved to: " + e.NewPosition.X +
                 ", " + e.NewPosition.Y);

        private static void Mouse_ButtonChanged(GHIElectronics.TinyCLR.Devices.UsbHost.Mouse
            sender, GHIElectronics.TinyCLR.Devices.UsbHost.Mouse.ButtonChangedEventArgs args) => System.Diagnostics.Debug.WriteLine
                ("Mouse button changed: " + ((object)args.Which).ToString());


    }
}
