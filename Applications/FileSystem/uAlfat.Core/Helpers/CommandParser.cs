using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace uAlfat.Core {
    /// <summary>
    /// parse uart command to understand context
    /// </summary>
    public class CommandParser {
        public int ParamLength { get; set; } = 0;
        public string CommandPrefix { get; set; }
        public string[] Parameters { get; set; }
        public string NextLine { get; set; }
        public static CommandParser Parse(string data) {
            if (!string.IsNullOrEmpty(data)) {

                var splitLine = Regex.Split("\n", data, RegexOptions.IgnoreCase);
                data = splitLine[0];
                var nextLine = string.Empty;
                if (splitLine.Length > 1)
                    nextLine = splitLine[1];
                var splitted = data.Split(' ');

                if (splitted != null && splitted.Length == 3 && splitted[0][0] == 'R') {
                    // special 'R' command accept space
                    splitted = data.Split(new char[] { ' ' }, 2);
                }

                if (splitted.Length > 0) {
                    var param = new string[splitted.Length - 1];
                    for (var i = 1; i < splitted.Length; i++) {
                        param[i - 1] = splitted[i];
                    }
                    return new CommandParser() { CommandPrefix = splitted[0], Parameters = param, ParamLength = param.Length, NextLine = nextLine };
                }
            }
            return new CommandParser() { CommandPrefix = string.Empty, Parameters = null };
        }
    }
}
