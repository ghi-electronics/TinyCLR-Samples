using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class ResponseCode
    {
        public static string Success = "!00";
        
        public static string FailToWrite = "!03";
        public static string EndOfFile = "!04";
        public static string MediaNotInitialize = "!10";
        public static string MediaInitFailed = "!11";
        public static string InsufficientFreeSpace = "!12";
        
        public static string FailToOpen = "!21";
        public static string SeekingMustBeOpen = "!22";
        public static string SeekingOutsideFileSize = "!23";
        public static string FileNameZero = "!24";
        public static string ForbiddenCharacter = "!25";
        public static string FileFolderExists = "!26";
        public static string InvalidHandle = "!30";
        public static string HandleSourceCannotOpen = "!31";
        public static string HandleDestCannotOpen = "!32";
        public static string HandleSourceNeedOpen = "!33";
        public static string HandleDestNeedOpen = "!34";
        public static string NoMoreHandles = "!35";
        public static string HandleCannotBeOpen = "!36";
        public static string HandleAlreadyUsed = "!37";
        public static string InvalidOpenMode = "!38";
        public static string HandleRequireAppend = "!39";
        public static string HandleRequireRead = "!3A";
        public static string Busy = "!40";
        public static string OnlySPI = "!41";
        public static string FlushSignalDetected = "!60";
        public static string NoSDCard = "!70";
        
        public static string KeyboardNotDetect = "!71";
        public static string KeyboardNotInit = "!72";

        public static string ERROR_COMMANDER_INCORRECT_CMD_PARAMETER = "!68";
        public static string ERROR_COMMANDER_NO_USB = "!7D";
        public static string ERROR_ENDOF_FILEFOLDER_LIST = "!4D";
        public static string ERROR_FAILED_OPEN_FILE = "!46";
        public static string ERROR_FS_SEEK_READ_ONLY = "!48";
        public static string ERROR_FS_SEEK_OUTOF_LENGTH = "!49";
        public static string ERROR_FS_FILEFOLDER_NOT_EXIST = "!41";

        public static string ERROR_USB_INITIALIZE_FAILED = "!81";

        public static string ERROR_MEDIA_NOT_INITIALIZED = "!02";

    }
}
