using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class Strings
    {
        public const string NewLine = "\r";

        #region Public Methods
        /// <summary>
        /// leading string data with zero
        /// </summary>
        /// <param name="data">string data</param>
        /// <param name="count">length of string data</param>
        /// <returns></returns>
        public static string LeadingZero(string data, int count)
        {
            if (count < data.Length) return data;
            var fills = string.Empty;
            for (var i = 0; i < count-data.Length; i++)
            {
                fills += "0";
            }
            return fills+data;
        }
        /// <summary>
        /// add filler to the end of data string
        /// </summary>
        /// <param name="filler">filler character</param>
        /// <param name="count">number of filler</param>
        /// <returns></returns>
        public static string GetFiller(char filler, long count)
        {
            if (count < 1) return "";
            var fills = string.Empty;
            for(var i = 0; i < count; i++)
            {
                fills += filler;
            }
            return fills;
        }
        /// <summary>
        /// Return absolute path from relative path
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string AbsolutePath(string baseDirectory, string target)
        {
            int i;

            // Remove last slash
            if (baseDirectory.Substring(baseDirectory.Length - 1) == "\\")
                baseDirectory = baseDirectory.Substring(0, baseDirectory.Length - 1);

            // Check roots
            if (target.Substring(0, 1) == "\\")
                return target;

            // Remove upline
            while (target.IndexOf("..\\") >= 0)
            {
                i = baseDirectory.LastIndexOf("\\");

                // Invalid Path
                if (i <= 0)
                    return target;

                baseDirectory = baseDirectory.Substring(0, i);
                target = target.Substring(3);
            }

            return NormalizeDirectory(baseDirectory) + target;
        }

        /// <summary>
        /// Return X.X Byte/KB/MB/GB/TB
        /// </summary>
        /// <param name="value">Size</param>
        /// <returns></returns>
        public static string FormatDiskSize(long value)
        {
            var cur = (double)value;
            var size = new string[] { "bytes", "kb", "mb", "gb", "tb" };
            var i = 0;

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
            var qcount = 0;
            var iStart = 0;

            while (true)
            {
                // Find next instance of a quote
                var i = value.IndexOf('"', iStart);

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
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizeDirectory(string path)
        {
            if (path.Substring(path.Length - 1) != "\\") return path + "\\";
            return path;
        }

        public static string PadZeroLeft(int value, int length)
        {
            var s = value.ToString();
            while (s.Length < length)
                s = "0" + s;
            return s;
        }

        /// <summary>
        /// Get a relative path for a file or folder
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string RelativePath(string baseDirectory, string target)
        {
            var sRes = string.Empty;
            int i, e, s;

            // Check for file only
            if (target.IndexOf('\\') < 0)
                return target;

            // Split strings
            var rel1 = NormalizeDirectory(baseDirectory).Split('\\');
            var rel2 = target.Split('\\');

            // Check for different drive
            if (rel1[1].ToLower() != rel2[1].ToLower())
                return target;

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
            if (Path.GetExtension(target) != string.Empty)
                sRes = sRes.Substring(0, sRes.Length - 1);

            // Return outcome
            return sRes;
        }

        /// <summary>
        /// Finds and replaces occurances within a string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toFind"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string Replace(string source, string toFind, string replaceWith)
        {
            int i;
            var iStart = 0;

            if (source == string.Empty || source == null || toFind == string.Empty || toFind == null)
                return source;

            while (true)
            {
                i = source.IndexOf(toFind, iStart);
                if (i < 0) break;

                if (i > 0)
                    source = source.Substring(0, i) + replaceWith + source.Substring(i + toFind.Length);
                else
                    source = replaceWith + source.Substring(i + toFind.Length);

                iStart = i + replaceWith.Length;
            }
            return source;
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
            var iStart = 0;
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
            var res = SplitComponents(command, ' ');
            for (var i = 0; i < res.Length; i++)
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
