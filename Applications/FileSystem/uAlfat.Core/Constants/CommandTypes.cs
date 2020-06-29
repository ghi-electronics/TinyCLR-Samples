using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class CommandTypes
    {
        /// <summary>
        /// for init media
        /// </summary>
        public const string Init = "I";

        /// <summary>
        /// for mount usb
        /// </summary>
        public const string MountUsb = "U";

        /// <summary>
        /// for detect usb
        /// </summary>
        public const string DetectUsb = "J";

        /// <summary>
        /// for open file
        /// </summary>
        public const string Open = "O";

        /// <summary>
        /// for close file
        /// </summary>
        public const string Close = "C";

        /// <summary>
        /// for read file
        /// </summary>
        public const string Read = "R";

        /// <summary>
        /// for write file
        /// </summary>
        public const string Write = "W";

        /// <summary>
        /// for tell file (query read position)
        /// </summary>
        public const string Tell = "Y";

        /// <summary>
        /// for seek file 
        /// </summary>
        public const string Seek = "P";

        /// <summary>
        /// for delete file 
        /// </summary>
        public const string Delete = "D";

        /// <summary>
        /// for listing files in a folder 
        /// </summary>
        public const string FileListing = "@";

        /// <summary>
        /// for next result 
        /// </summary>
        public const string NextResult = "N";

        /// <summary>
        /// for next result long file name
        /// </summary>
        public const string NextResult2 = "L";

        /// <summary>
        /// for get time/date
        /// </summary>
        public const string GetDateTime = "G";

        /// <summary>
        /// for set time/date
        /// </summary>
        public const string SetDateTime = "S";

        /// <summary>
        /// for init RTC
        /// </summary>
        public const string InitTimer = "T";

        /// <summary>
        /// for find file/folder
        /// </summary>
        public const string FindFile = "?";

        /// <summary>
        /// for change dir
        /// </summary>
        public const string ChangeDirectory = "A";


        /// <summary>
        /// for make dir
        /// </summary>
        public const string MakeDirectory = "M";

        /// <summary>
        /// for get version
        /// </summary>
        public const string GetVersionNo = "V";

        /// <summary>
        /// for set baud rate
        /// </summary>
        public const string SetBaudRate = "B";

        /// <summary>
        /// for sleep/stop
        /// </summary>
        public const string Sleep = "Z";

        /// <summary>
        /// for enabled/disable echo
        /// </summary>
        public const string SetEcho = "#";

        /// <summary>
        /// for get media stat
        /// </summary>
        public const string GetMediaStatistic = "K";

        /// <summary>
        /// for format drive
        /// </summary>
        public const string FormatDrive = "Q";

        /// <summary>
        /// for set power mode (uAlfat)
        /// </summary>
        public const string SetPowerMode = "Z";

        /// <summary>
        /// for delete folder
        /// </summary>
        public const string DeleteFolder = "E";

        /// <summary>
        /// for Flush media
        /// </summary>
        public const string Flush = "F";

    }
}
