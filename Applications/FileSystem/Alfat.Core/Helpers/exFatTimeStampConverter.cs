using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class ExFatTimeStampConverter
    {
        public static string ConvertToFatTime(DateTime timestamp)
        {
            var result = string.Empty;
            var year = timestamp.Year - 1980;
            var yearBit = Strings.LeadingZero(IntToBinaryString(year), 7);
            var monthBit = Strings.LeadingZero(IntToBinaryString(timestamp.Month), 4);
            var lineOne = yearBit + monthBit[0];
            var dayBit = Strings.LeadingZero(IntToBinaryString(timestamp.Day), 5);
            var lineTwo = monthBit.Substring(1, monthBit.Length - 1) + dayBit;
            var hourBit = Strings.LeadingZero(IntToBinaryString(timestamp.Hour), 5);
            var minBit = Strings.LeadingZero(IntToBinaryString(timestamp.Minute), 6);
            var secBit = Strings.LeadingZero(IntToBinaryString(timestamp.Second / 2), 5);
            var lineThree = hourBit + minBit.Substring(0, 3);
            var lineFour = minBit.Substring(3, minBit.Length - 3) + secBit;
            result = $"{ByteToHex(BitStringToInt(lineOne))}{ByteToHex(BitStringToInt(lineTwo))}" +
                $"{ByteToHex(BitStringToInt(lineThree))}{ByteToHex(BitStringToInt(lineFour))}";
            return result;

        }
        public static string ByteToHex(int b)
        {
            const string Hex = "0123456789ABCDEF";
            var lowNibble = b & 0x0F;
            var highNibble = (b & 0xF0) >> 4;
            var s = new string(new char[] { Hex[highNibble], Hex[lowNibble] });
            return s;
        }
        public static DateTime ConvertToDatetime(string timestamp)
        {
            try
            {
                var firstLine = Hex2Binary("0x" + timestamp.Substring(0, 2));
                firstLine = Strings.LeadingZero(firstLine, 8);
                //convert binary to int
                var year = BitStringToInt(firstLine.Substring(0, 7)) + 1980;
                //leading zero 2 ( 8 chars)
                var secondLine = Hex2Binary("0x" + timestamp.Substring(2, 2));
                secondLine = Strings.LeadingZero(secondLine, 8);
                var monthStr = firstLine[firstLine.Length - 1] + secondLine.Substring(0, 3);
                var month = BitStringToInt(monthStr);

                var dayStr = secondLine.Substring(3, 5);
                var day = BitStringToInt(dayStr);

                var thirdLine = Hex2Binary("0x" + timestamp.Substring(4, 2));
                thirdLine = Strings.LeadingZero(thirdLine, 8);
                var hourStr = thirdLine.Substring(0, 5);
                var hour = BitStringToInt(hourStr);

                var fourthLine = Hex2Binary("0x" + timestamp.Substring(6, 2));
                fourthLine = Strings.LeadingZero(fourthLine, 8);
                var minStr = thirdLine.Substring(5, 3) + fourthLine.Substring(0, 3);
                var min = BitStringToInt(minStr);


                var secondStr = fourthLine.Substring(3, 5);
                var second = BitStringToInt(secondStr) * 2;

                var newDate = new DateTime(year, month, day, hour, min, second);

                return newDate;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        public static string ReverseString(string s)
        {
            var array = new char[s.Length];
            var forward = 0;
            for (var i = s.Length - 1; i >= 0; i--)
            {
                array[forward++] = s[i];
            }
            return new string(array);
        }
        public static int BitStringToInt(string bits)
        {
            var reversedBits = ReverseString(bits).ToCharArray();
            var num = 0;
            for (var power = 0; power < reversedBits.Length; power++)
            {
                var currentBit = reversedBits[power];
                if (currentBit == '1')
                {
                    var currentNum = (int)Math.Pow(2, power);
                    num += currentNum;
                }
            }

            return num;
        }
        public static string IntToBinaryString(int number)
        {
            const int Mask = 1;
            var binary = string.Empty;
            while (number > 0)
            {
                // Logical AND the number and prepend it to the result string
                binary = (number & Mask) + binary;
                number = number >> 1;
            }

            return binary;
        }
        private static string Hex2Binary(string hexvalue)
        {
            var binaryval = "";
            binaryval = IntToBinaryString(Convert.ToInt32(hexvalue, 16));
            return binaryval;
        }
        public static string ByteConvert(int num)
        {
            var p = new int[8];
            var pa = "";
            for (var ii = 0; ii <= 7; ii = ii + 1)
            {
                p[7 - ii] = num % 2;
                num = num / 2;
            }
            for (var ii = 0; ii <= 7; ii = ii + 1)
            {
                pa += p[ii].ToString();
            }
            return pa;
        }
    }
}
