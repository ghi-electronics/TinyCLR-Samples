using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class Strings
    {
        public const string NewLine = "\n";

        #region Public Methods
        /// <summary>
        /// Leading string data by zero
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static string LeadingZero(string Data, int Count)
        {
            if (Count < Data.Length) return "";
            string Fills = string.Empty;
            for (int i = 0; i < Count-Data.Length; i++)
            {
                Fills += "0";
            }
            return Fills+Data;
        }
        /// <summary>
        /// Add filler to string data in the end
        /// </summary>
        /// <param name="Filler"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static string GetFiller(char Filler, long Count)
        {
            if (Count < 1) return "";
            string Fills = string.Empty;
            for(int i = 0; i < Count; i++)
            {
                Fills += Filler;
            }
            return Fills;
        }
        /// <summary>
        /// Return absolute path from relative path
        /// </summary>
        /// <param name="BaseDirectory"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public static string AbsolutePath(string BaseDirectory, string Target)
        {
            int i;

            // Remove last slash
            if (BaseDirectory.Substring(BaseDirectory.Length - 1) == "\\")
                BaseDirectory = BaseDirectory.Substring(0, BaseDirectory.Length - 1);

            // Check roots
            if (Target.Substring(0, 1) == "\\")
                return Target;

            // Remove upline
            while (Target.IndexOf("..\\") >= 0)
            {
                i = BaseDirectory.LastIndexOf("\\");

                // Invalid Path
                if (i <= 0)
                    return Target;

                BaseDirectory = BaseDirectory.Substring(0, i);
                Target = Target.Substring(3);
            }

            return NormalizeDirectory(BaseDirectory) + Target;
        }

        /// <summary>
        /// Return X.X Byte/KB/MB/GB/TB
        /// </summary>
        /// <param name="value">Size</param>
        /// <returns></returns>
        public static string FormatDiskSize(long value)
        {
            double cur = (double)value;
            string[] size = new string[] { "bytes", "kb", "mb", "gb", "tb" };
            int i = 0;

            while (cur > 1024 && i < 4)
            {
                cur /= 1024;
                i++;
            }

            return System.Math.Round(cur) + size[i];
        }

        /// <summary>
        /// Determine if a specific character is inside of a quote string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool InQuotes(string value, int position)
        {
            int qcount = 0;
            int i;
            int iStart = 0;

            while (true)
            {
                // Find next instance of a quote
                i = value.IndexOf('"', iStart);

                // If not return our value
                if (i < 0 || i >= position)
                    return qcount % 2 != 0;

                // Check if it's a qualified quote
                if (i > 0 && value.Substring(i, 1) != "\\" || i == 0)
                    qcount++;

                iStart = i + 1;
            }
        }

        public static string MonthToShortString(int month)
        {
            switch (month)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    return null;
            }

        }

        public static string MonthToString(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    return null;
            }

        }

        /// <summary>
        /// Ensures the directory ends with a '\' character
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string NormalizeDirectory(string Path)
        {
            if (Path.Substring(Path.Length - 1) != "\\") return Path + "\\";
            return Path;
        }

        public static string PadZeroLeft(int value, int length)
        {
            string s = value.ToString();
            while (s.Length < length)
                s = "0" + s;
            return s;
        }

        /// <summary>
        /// Get a relative path for a file or folder
        /// </summary>
        /// <param name="BaseDirectory"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public static string RelativePath(string BaseDirectory, string Target)
        {
            string sRes = string.Empty;
            int i, e, s;

            // Check for file only
            if (Target.IndexOf('\\') < 0)
                return Target;

            // Split strings
            string[] rel1 = NormalizeDirectory(BaseDirectory).Split('\\');
            string[] rel2 = Target.Split('\\');

            // Check for different drive
            if (rel1[1].ToLower() != rel2[1].ToLower())
                return Target;

            // Find last match
            s = 0;
            e = (rel1.Length < rel2.Length) ? rel1.Length : rel2.Length;
            for (i = 2; i < e; i++)
            {
                if (rel1[i].ToLower() != rel2[i].ToLower())
                {
                    s = i;
                    break;
                }
            }

            // Build upline
            if (s < rel1.Length - 1)
            {
                for (i = s; i < rel1.Length; i++)
                {
                    if (rel1[i] != string.Empty)
                        sRes += "..\\";
                }
            }

            // Build downline
            if (s < rel2.Length)
            {
                for (i = s; i < rel2.Length; i++)
                    sRes += rel2[i] + "\\";
            }

            // Check for file
            if (Path.GetExtension(Target) != string.Empty)
                sRes = sRes.Substring(0, sRes.Length - 1);

            // Return outcome
            return sRes;
        }

        /// <summary>
        /// Finds and replaces occurances within a string
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="ToFind"></param>
        /// <param name="ReplaceWith"></param>
        /// <returns></returns>
        public static string Replace(string Source, string ToFind, string ReplaceWith)
        {
            int i;
            int iStart = 0;

            if (Source == string.Empty || Source == null || ToFind == string.Empty || ToFind == null)
                return Source;

            while (true)
            {
                i = Source.IndexOf(ToFind, iStart);
                if (i < 0) break;

                if (i > 0)
                    Source = Source.Substring(0, i) + ReplaceWith + Source.Substring(i + ToFind.Length);
                else
                    Source = ReplaceWith + Source.Substring(i + ToFind.Length);

                iStart = i + ReplaceWith.Length;
            }
            return Source;
        }

        public static string ReplaceEmptyOrNull(string value, string replaceWith)
        {
            if (value == string.Empty || value == null)
                return replaceWith;
            return value;
        }

        public static string ReplaceEmptyOrNull(object value, string replaceWith)
        {
            if (value == null || value.ToString() == string.Empty)
                return replaceWith;
            return value.ToString();
        }

        /// <summary>
        /// Split a string by deliminator
        /// </summary>
        /// <param name="value"></param>
        /// <param name="deliminator"></param>
        /// <returns></returns>
        public static string[] SplitComponents(string value, char deliminator)
        {
            int iStart = 0;
            string[] ret = null;
            string[] tmp;
            int i;
            string s;

            while (true)
            {
                // Find deliminator
                i = value.IndexOf(deliminator, iStart);

                if (InQuotes(value, i))
                    iStart = i + 1;
                else
                {
                    // Separate value
                    if (i < 0)
                        s = value;
                    else
                    {
                        s = value.Substring(0, i).Trim();
                        value = value.Substring(i + 1);
                    }

                    // Add value
                    if (ret == null)
                        ret = new string[] { s };
                    else
                    {
                        tmp = new string[ret.Length + 1];
                        Array.Copy(ret, tmp, ret.Length);
                        tmp[tmp.Length - 1] = s;
                        ret = tmp;
                    }

                    iStart = 0;
                }

                // Break on last value
                if (i < 0 || value == string.Empty)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Tokenize a string
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string[] Tokenize(string command)
        {
            string[] res = SplitComponents(command, ' ');
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = res[i].Trim();
                if (res[i].Substring(0, 1) == "\"" && res[i].Substring(res[i].Length - 1) == "\"")
                    res[i] = res[i].Substring(1, res[i].Length - 2);
            }
            return res;
        }

        public static string WeekdayToShortString(int weekday)
        {
            switch (weekday)
            {
                case 1:
                    return "Su";
                case 2:
                    return "Mo";
                case 3:
                    return "Tu";
                case 4:
                    return "We";
                case 5:
                    return "Th";
                case 6:
                    return "Fr";
                case 7:
                    return "Sa";
                default:
                    return null;
            }

        }

        #endregion

    }
}
