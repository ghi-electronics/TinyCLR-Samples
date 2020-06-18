using Alfat.Core.Properties;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.IO;
using System.Text;

namespace Alfat.Core {
    public class AlfatModule
    {
        public int LDRPin { get; set; }
        public static bool IsEchoEnabled { get; set; }
        public RTCModes RTCMode { get; set; }
        public enum RTCModes { Shared, Backup };
        static FileExplorer fileExplorer;
        static ActiveHandle activeHandle;
        static MediaHandler handles;
        static StorageContainer storages;
        string SDControllerName { get; set; }
        public bool IsUsbDiskConnected { get; set; }
        public bool IsSDConnected { get; set; }
        public bool IsKeyboardConnected { get; set; }
        string StorageControllerName { get; set; }
        public static GHIElectronics.TinyCLR.Devices.UsbHost.UsbHostController UsbHost { set; get; }
        public static CommunicationsBus Bus { get; set; }
        public AlfatModule(string uartPort,string storageControllerName,string sDControllerName, int ldrPin = SC20260.GpioPin.PE3)
        {
            this.LDRPin = ldrPin;
            IsEchoEnabled = false;
            this.RTCMode = RTCModes.Backup;
            fileExplorer = new FileExplorer();
            activeHandle = new ActiveHandle() { Mode=ActiveHandle.HandleMode.Idle };
            handles = new MediaHandler();
            storages = new StorageContainer();
            Bus = new CommunicationsBus(uartPort);
            Bus.DataReceived += this.ProcessCommand;
            this.StorageControllerName = storageControllerName;
            this.SDControllerName = sDControllerName;
            this.InitUsbHost();
            Console.WriteLine("Alfat is ready");
            this.PrintStartUpMessage();
        }
        void PrintStartUpMessage() {
            var appVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var bootVer = Resources.GetString(Resources.StringResources.BOOTLOADER_VER);
            Bus.WriteLine($"GHI Electronics, LLC{Strings.NewLine} ----------------------------- {Strings.NewLine} Boot Loader {bootVer} {Strings.NewLine} ALFAT(TM) {appVer} {Strings.NewLine}{ResponseCode.Success}");
            Console.WriteLine($"GHI Electronics, LLC{Strings.NewLine} ----------------------------- {Strings.NewLine} Boot Loader {bootVer} {Strings.NewLine} ALFAT(TM) {appVer} {Strings.NewLine}{ResponseCode.Success}");
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
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split(':');
                        if (par1.Length > 0)
                        {
                            switch (par1[0])
                            {
                                case MediaTypes.D:
                                case MediaTypes.M:
                                    isSuccess = this.ConnectSD(par1[0]);
                                    if (isSuccess)
                                    {
                                        result = ResponseCode.Success;
                                    }
                                    else
                                    {
                                        result = ResponseCode.NoSDCard;
                                    }
                                    break;
                                case MediaTypes.U0:
                                case MediaTypes.U1:
                                    if (this.IsUsbDiskConnected)
                                    {
                                        isSuccess = true;
                                        result = ResponseCode.Success;
                                    }
                                    else
                                    {
                                        result = ResponseCode.FailToOpen;
                                    }
                                    break;
                                case MediaTypes.K0:
                                case MediaTypes.K1:
                                    if (this.IsKeyboardConnected)
                                    {
                                        isSuccess = true;
                                        result = ResponseCode.Success;
                                    }
                                    else
                                    {
                                        result = ResponseCode.KeyboardNotDetect;
                                    }
                                    break;
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
                case CommandTypes.Open:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split(':');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var fileName = par1[1];
                                var media = par1[0].Split('>')[1];
                                var handle = par1[0].Split('>')[0][0];
                                var accessType = par1[0].Split('>')[0][1];
                               
                                //is handle available
                                if (handles.IsExist(handle))
                                {
                                    result = ResponseCode.HandleAlreadyUsed;
                                }
                                else
                                {
                                    //has storage been init ?
                                    var storage = storages.GetStorage(media);
                                    if (storage == null)
                                    {
                                        result = ResponseCode.MediaNotInitialize;
                                    }
                                    else
                                    {
                                        var newHandle = new MediaHandle() { AccessType = accessType, HandleName = handle, FileName = $"{storage.DriveLetter}:{fileName}", Media = media };
                                        fileName = $"{storage.DriveLetter}:{fileName}";
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
                                try
                                {
                                    //flush memory to file
                                    File.WriteAllBytes(currentHandle.FileName, byteToWrite);
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
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
                                                var lengthRead = currentHandle.Buffer.Length - currentHandle.CursorPosition;//numReadBytes - currentHandle.Buffer.Length;
                                                var readBytes = new byte[lengthRead];
                                                currentHandle.Buffer.Seek(currentHandle.CursorPosition, SeekOrigin.Begin);
                                                for (var i = 0; i < lengthRead; i++)
                                                {
                                                    readBytes[i] = (byte)currentHandle.Buffer.ReadByte();
                                                }
                                                //go to beginning of file
                                              
                                                var numberOfPad = (numReadBytes + currentHandle.CursorPosition) - currentHandle.Buffer.Length;
                                                currentHandle.CursorPosition = 0;
                                                var contentStr = Encoding.UTF8.GetString(readBytes);
                                                contentStr += Strings.GetFiller(pad, numberOfPad);
                                               
                                                result += contentStr + Strings.NewLine;
                                              
                                                //write actual bytes
                                                var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", contentStr.Length), 8);
                                              
                                                result += actBytes + Strings.NewLine;
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
                                                var contentStr = Encoding.UTF8.GetString(readBytes);
                                                //write content
                                                result += contentStr + Strings.NewLine;
                                               
                                                //write actual bytes
                                                var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", numReadBytes), 8);
                                             
                                                result += actBytes + Strings.NewLine;

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
                        par1 = cmd.Parameters[0].Split(':');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var fileName = par1[1];
                                var media = par1[0];

                                var storage = storages.GetStorage(media);
                                if (storage == null)
                                {
                                    result = ResponseCode.MediaNotInitialize;
                                }
                                else
                                {
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

                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
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


                    }
                    break;
                case CommandTypes.FileListing:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split(':');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var folderName = par1[1];
                                var media = par1[0];

                                var storage = storages.GetStorage(media);
                                if (storage == null)
                                {
                                    result = ResponseCode.MediaNotInitialize;
                                }
                                else
                                {
                                    var dir1 = new DirectoryInfo(folderName);
                                    if (dir1.Exists)
                                    {
                                        fileExplorer.CurrentDirectory = folderName;
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
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
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
                                //file/folder name
                              
                                result += $"{item.Name}{Strings.NewLine}";
                                //attributes
                                var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Attribute), 2);
                                
                                result += $"{actBytes}{Strings.NewLine}";
                                //size
                                actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", item.Size), 8);
                               
                                result += $"{actBytes}{Strings.NewLine}";
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
                        switch (request)
                        {
                            case 'D':
                                Bus.WriteLine(DateTime.Now.ToString("MM-dd-yyyy"));
                                break;
                            case 'T':
                                Bus.WriteLine(DateTime.Now.ToString("hh:mm:ss"));
                                break;
                        }

                        Bus.WriteLine(ResponseCode.Success);

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
                            //conversion process here..
                            var newdate = ExFatTimeStampConverter.ConvertToDatetime(newdatestr);
                            if (this.RTCMode == RTCModes.Backup)
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
                case CommandTypes.InitRTC:
                    if (cmd.ParamLength > 0)
                    {
                        var mode = cmd.Parameters[0].Trim()[0];
                        this.RTCMode = mode == 'S' ? RTCModes.Shared : RTCModes.Backup;
                        Bus.WriteLine(ResponseCode.Success);

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.GetStatus:
                    if (cmd.ParamLength > 0)
                    {
                        var status = cmd.Parameters[0].Trim()[0];
                        var response = "0"; //default response
                        Bus.WriteLine($"{ResponseCode.Success}{Strings.NewLine}{response}{Strings.NewLine}{ResponseCode.Success}");

                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.FindFile:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split(':');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var itemName = par1[1];
                                var media = par1[0];

                                var storage = storages.GetStorage(media);
                                if (storage == null)
                                {
                                    result = ResponseCode.MediaNotInitialize;
                                }
                                else
                                {
                                    //check if File 
                                    if (File.Exists(itemName))
                                    {
                                        var file = new FileInfo(itemName);
                                        result += $"{ResponseCode.Success}{Strings.NewLine}";

                                        var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", file.Length), 8);
                                        result += $"{actBytes}{Strings.NewLine}";

                                        actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", ItemResult.GetAttribute(file.Attributes)), 2);
                                        result += $"{actBytes}{Strings.NewLine}";

                                        result += $"${file.LastWriteTime.ToString("HH:mm:ss MM/dd/yyyy")}{Strings.NewLine}";

                                        result += ResponseCode.Success;
                                    }
                                    else if (Directory.Exists(itemName)) //check if Directory
                                    {
                                        var dir = new DirectoryInfo(itemName);
                                        result += $"{ResponseCode.Success}{Strings.NewLine}";

                                        var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", 0), 8);
                                        result += $"{actBytes}{Strings.NewLine}";

                                        actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", ItemResult.GetAttribute(dir.Attributes)), 2);
                                        result += $"{actBytes}{Strings.NewLine}";

                                        result += $"${dir.LastWriteTime.ToString("HH:mm:ss MM/dd/yyyy")}{Strings.NewLine}";

                                        result += ResponseCode.Success;

                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
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


                    }
                    break;
                case CommandTypes.RenameFile:
                    if (cmd.ParamLength > 0)
                    {
                        par1 = cmd.Parameters[0].Split('>');

                        if (par1.Length > 0)
                        {
                            try
                            {
                                var destination = par1[1].Trim();
                                var media = par1[0].Split(':')[0];
                                var filename = par1[0].Split(':')[1];
                                var storage = storages.GetStorage(media);
                                if (storage == null)
                                {
                                    result = ResponseCode.MediaNotInitialize;
                                }
                                else
                                {
                                    var info = new FileInfo(filename);
                                    if (info.Exists)
                                    {
                                        File.Move(info.FullName, info.Directory.FullName + "\\" + destination);
                                        result = ResponseCode.Success;
                                    }
                                    else
                                    {
                                        result = ResponseCode.FileFolderNotExist;
                                    }
                                }
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
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.GetFreeSize:
                    if (cmd.ParamLength > 0)
                    {
                        var media = Strings.Replace(cmd.Parameters[0], ":", string.Empty);
                        var storage = storages.GetStorage(media);
                        if (storage == null)
                        {
                            result = ResponseCode.MediaNotInitialize;
                        }
                        else
                        {
                            var info = new DriveInfo(storage.Drive.Name);
                            result += $"{ResponseCode.Success}{Strings.NewLine}";
                            //size in sectors, divided by 512
                            var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", info.TotalFreeSpace/512), 16);
                            result += $"{actBytes}{Strings.NewLine}";
                            result += ResponseCode.Success;
                        }
                        Bus.WriteLine(result);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.CopyFile:
                    if (cmd.ParamLength == 4)
                    {
                        var handleSrc = handles.GetHandle(cmd.Parameters[0][0]);
                        var startPosition = long.Parse(cmd.Parameters[1]);
                        var handleDest = handles.GetHandle(cmd.Parameters[2][0]);
                        var dataSize = long.Parse(cmd.Parameters[3]);

                        if (handleSrc != null && handleDest != null)
                        {
                            //src must be read
                            if (handleSrc.AccessType != FileAccessTypes.Read)
                            {
                                result = ResponseCode.HandleRequireRead;
                            }
                            //dst must be write/append
                            else if (handleDest.AccessType != FileAccessTypes.Write && handleDest.AccessType != FileAccessTypes.Append)
                            {
                                result = ResponseCode.HandleRequireAppend;
                            }
                            else
                            {
                                if (startPosition < 0)
                                {
                                    result = ResponseCode.IncorrectParameter;
                                }
                                else if (startPosition + dataSize > handleSrc.Size)
                                {
                                    result = ResponseCode.SeekingOutsideFileSize;
                                }
                                else
                                {
                                    result += $"{ResponseCode.Success}{Strings.NewLine}";
                                    handleSrc.Buffer.Seek(startPosition, SeekOrigin.Begin);
                                    for (var i = 0; i < dataSize; i++)
                                    {
                                        //read from source
                                        handleSrc.CursorPosition = startPosition + i;
                                        //write to dest
                                        handleDest.Buffer.WriteByte((byte)handleSrc.Buffer.ReadByte());
                                        handleDest.CursorPosition++;
                                        handleDest.Size = handleDest.CursorPosition + 1;
                                    }
                                    var actBytes = "$" + Strings.LeadingZero(string.Format("{0:X}", dataSize), 8);
                                    result += $"{actBytes}{Strings.NewLine}";
                                    result += ResponseCode.Success;
                                }
                            }


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
                case CommandTypes.GetVersionNo:
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    result = $"V{version}{Strings.NewLine}";
                    result += ResponseCode.Success;
                    Bus.WriteLine(result);

                    break;
                case CommandTypes.SetBaudRate:
                    if (cmd.ParamLength > 0)
                    {
                        var newBaudRate = Convert.ToInt32("0x" + cmd.Parameters[0].Trim(), 16);
                        Bus.WriteLine(ResponseCode.Success);
                        Bus.SetBaudRate(newBaudRate);
                        Bus.WriteLine(ResponseCode.Success);
                    }
                    else
                    {
                        Bus.WriteLine(ResponseCode.IncorrectParameter);
                    }
                    break;
                case CommandTypes.FormatDrive:
                    if (cmd.ParamLength == 3)
                    {
                        var media = Strings.Replace( cmd.Parameters[2].Trim(),":",string.Empty);
                        var storage = storages.GetStorage(media);
                        if (storage == null)
                        {
                            result = ResponseCode.MediaNotInitialize;
                        }
                        else
                        {
                            //code for formatting drive here...
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
                case CommandTypes.Sleep:
                    if (cmd.ParamLength > 0)
                    {
                        var pars = cmd.Parameters[0].Split('>');
                        var param1 = int.Parse(pars[0]);
                        if (pars.Length > 1 && param1 == 2)
                        {
                            //switch wake up pin function
                            Bus.WriteLine(ResponseCode.Success);
                        }
                        else
                        {
                            switch (param1)
                            {
                                case 0:
                                    {
                                        //for now it's hardcoded
                                        var ldrButton = GpioController.GetDefault().OpenPin(this.LDRPin);
                                        ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
                                        ldrButton.ValueChanged += (GpioPin sender, GpioPinValueChangedEventArgs e) =>{ };
                                        Bus.WriteLine(ResponseCode.Success);
                                        //The next line starts Sleep.
                                        Power.Sleep();
                                       //I don't know how to reset after wake up...
                                    }
                                    break;
                                case 1:
                                    
                                    {
                                        //for now it's hardcoded
                                        var ldrButton = GpioController.GetDefault().OpenPin(this.LDRPin);
                                        ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
                                        ldrButton.ValueChanged += (GpioPin sender, GpioPinValueChangedEventArgs e) => { };
                                        Bus.WriteLine(ResponseCode.Success);
                                        //The next line starts Sleep.
                                        Power.Sleep();
                                    }
                                    break;
                                case 3:
                                    Bus.WriteLine(ResponseCode.Success);
                                    Power.Reset();
                                    break;

                            }
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
                    storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = sd, Drive = drive, Name = mediaName });
                    this.IsSDConnected = true;
                }
                catch (Exception)
                {
                    this.IsSDConnected = false;
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
        private  void UsbHostController_OnConnectionChangedEvent
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

                            IDriveProvider driver;
                            try {
                                driver = GHIElectronics.TinyCLR.IO.FileSystem.
                                Mount(storageController.Hdc);
                            }
                            catch {
                                FileSystem.Unmount(storageController.Hdc);
                                driver = GHIElectronics.TinyCLR.IO.FileSystem.Mount(storageController.Hdc);
                            }
                            if (driver != null) {

                                var driveInfo = new System.IO.DriveInfo(driver.Name);
                                storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U0 });
                                storages.AddStorage(new StorageInfo() { DriveLetter = driveInfo.RootDirectory.FullName[0], Controller = storageController, Drive = driver, Name = MediaTypes.U1 });

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
                            }
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
                    //unmount if there is usb disk connected
                    if (this.IsUsbDiskConnected) {
                        var storageController = StorageController.FromName(this.StorageControllerName);
                        FileSystem.Unmount(storageController.Hdc);
                        //remove from list
                        storages.RemoveStorage(MediaTypes.U0);
                        storages.RemoveStorage(MediaTypes.U1);
                    }
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
