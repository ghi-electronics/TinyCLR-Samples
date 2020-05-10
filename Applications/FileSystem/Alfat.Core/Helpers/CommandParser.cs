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
        /// <param name="data"></param>
        /// <returns></returns>
        public static CommandParser Parse(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                
                var splitLine = Regex.Split("\n",data, RegexOptions.IgnoreCase);
                data = splitLine[0];
                var nextLine = string.Empty;
                if (splitLine.Length > 1)
                    nextLine = splitLine[1];
                var splitted = data.Split(' ');
                if (splitted.Length > 0)
                {
                    var param = new string[splitted.Length - 1];
                    for (var i = 1; i < splitted.Length; i++)
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
