using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Alfat.Core
{
    public class CommandParser
    {
        public int ParamLength { get; set; } = 0;
        public string CommandPrefix { get; set; }
        public string[] Parameters { get; set; }
        public string NextLine { get; set; }
        /// <summary>
        /// Split string command from user to understand the context
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static CommandParser Parse(string Data)
        {
            if (!string.IsNullOrEmpty(Data))
            {
                
                var splitLine = Regex.Split("\n",Data, RegexOptions.IgnoreCase);
                Data = splitLine[0];
                var nextLine = string.Empty;
                if (splitLine.Length > 1)
                    nextLine = splitLine[1];
                var splitted = Data.Split(' ');
                if (splitted.Length > 0)
                {
                    var param = new string[splitted.Length - 1];
                    for (int i = 1; i < splitted.Length; i++)
                    {
                        param[i - 1] = splitted[i];
                    }
                    return new CommandParser() { CommandPrefix = splitted[0], Parameters = param, ParamLength = param.Length, NextLine = nextLine };
                }
            }
            return new CommandParser() { CommandPrefix=string.Empty, Parameters=null };
        }
    }
}
