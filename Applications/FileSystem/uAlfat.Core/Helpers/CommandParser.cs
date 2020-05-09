using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace uAlfat.Core
{
    /// <summary>
    /// parse uart command to understand context
    /// </summary>
    public class CommandParser
    {
        public int ParamLength { get; set; } = 0;
        public string CommandPrefix { get; set; }
        public string[] Parameters { get; set; }
        public string NextLine { get; set; }
        public static CommandParser Parse(string Data)
        {
            if (!string.IsNullOrEmpty(Data))
            {
                
                var SplitLine = Regex.Split("\n",Data, RegexOptions.IgnoreCase);
                Data = SplitLine[0];
                var NextLine = string.Empty;
                if (SplitLine.Length > 1)
                    NextLine = SplitLine[1];
                var splitted = Data.Split(' ');
                if (splitted.Length > 0)
                {
                    var param = new string[splitted.Length - 1];
                    for (int i = 1; i < splitted.Length; i++)
                    {
                        param[i - 1] = splitted[i];
                    }
                    return new CommandParser() { CommandPrefix = splitted[0], Parameters = param, ParamLength = param.Length, NextLine = NextLine };
                }
            }
            return new CommandParser() { CommandPrefix=string.Empty, Parameters=null };
        }
    }
}
