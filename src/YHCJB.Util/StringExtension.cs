using System;
using System.Text.RegularExpressions;

namespace YHCJB.Util
{
    public static class StringExtension
    {
        public static void Print(this string str)
        {
            Console.Write(str);
        }

        public static void Println(this string str)
        {
            Console.WriteLine(str);
        }

        static readonly Regex _hzReg = new Regex(@"[\u4e00-\u9fa5]");
        public static string AlignRight(this string str, int length, char chr = ' ')
        {
            if (str != null)
            {
                var fillLength = length - _hzReg.Matches(str).Count - str.Length;
                return (fillLength > 0) ? new string(chr, fillLength) + str : str;
            }
            return new string(chr, length);
        }

        public static string AlignRight(this string str, string append, int length, char chr = ' ')
        {
            if (str == null) str = "";
            return str + append.AlignRight(length, chr);
        }

        public static string AlignLeft(this string str, int length, char chr = ' ')
        {
            if (str != null)
            {
                var fillLength = length - _hzReg.Matches(str).Count - str.Length;
                return (fillLength > 0) ? str + new string(chr, fillLength) : str;
            }
            return new string(chr, length);
        }

        public static string AlignLeft(this string str, string append, int length, char chr = ' ')
        {
            if (str == null) str = "";
            return str + append.AlignLeft(length, chr);
        }
    }
}
