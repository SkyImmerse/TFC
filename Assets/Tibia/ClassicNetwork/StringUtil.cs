using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public class StringUtil
    {
        public StringUtil()
        {
        }

        public static bool isEqualIgnoreCase(string a, string b)
        {
            return a.ToLower() == b.ToLower();
        }

        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool isWhitespace(char character)
        {
            char chr = character;
            switch (chr)
            {
                case '\t':
                case '\n':
                case '\f':
                case '\r':
                    {
                        return true;
                    }
                default:
                    {
                        if (chr == ' ')
                        {
                            return true;
                        }
                        return false;
                    }
            }
        }

        public static bool isWhitespace(string character)
        {
            if (string.IsNullOrEmpty(character))
            {
                return false;
            }
            char chr = character[0];
            switch (chr)
            {
                case '\t':
                case '\n':
                case '\f':
                case '\r':
                    {
                        return true;
                    }
                default:
                    {
                        if (chr == ' ')
                        {
                            return true;
                        }
                        return false;
                    }
            }
        }

        public static string substitute(string format, params object[] args)
        {
            string empty = string.Empty;
            try
            {
                empty = string.Format(format, args);
            }
            catch
            {
                //MUDebug.LogException(exception);
                empty = format;
            }
            return empty;
        }

        public static string trim(string str)
        {
            if (str == null || string.Empty == str)
            {
                return string.Empty;
            }
            int num = 0;
            while (StringUtil.isWhitespace(str[num]))
            {
                num++;
            }
            int length = str.Length - 1;
            while (StringUtil.isWhitespace(str[length]))
            {
                length--;
            }
            if (length < num)
            {
                return string.Empty;
            }
            return str.Substring(num, length + 1);
        }
    }
}
