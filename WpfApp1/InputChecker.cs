using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1
{
    public static class InputChecker
    {
        public static bool isValidInput(params string[] strings)
        {
            bool temp = true;
            foreach (var str in strings)
            {
                if (String.IsNullOrWhiteSpace(str) || IsRussian(str))
                {
                    temp = false;
                    break;
                }
            }

            return temp;
        }

        public static bool IsRussian(string str)
        {
            char[] chr = str.ToCharArray();
            for (int i = 0; i < chr.Length; i++)
            {
                if (chr[i] >= 'А' && chr[i] <= 'я')
                    return true;
            }
            return false;
        }
    }
}
