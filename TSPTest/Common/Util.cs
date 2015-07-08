using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSPTest.Common
{
    public class Util
    {
        private static string[] array = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "S", "Y", "Z" };

        public static string FormateDateTime(DateTime dt)
        {
            return dt.ToString("yyyy年MM月dd日 HH时mm分ss秒");
        }

        public static string FormateDateTime2(DateTime dt)
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static string FormateDateTime3(DateTime dt)
        {
            return dt.ToString("yyyy/MM/dd HH:mm:ss:fff");
        }

        public static string FormateDurationSecondsMaxHour(long seconds)
        {
            string msg = "";
            if (seconds / 3600 > 0)
                msg += (seconds / 3600) + "小时";
            msg += (seconds % 3600) / 60 + "分钟" + ((seconds % 3600) % 60) + "秒";
            return msg;
        }

        public static string FormateDurationSecondsMaxHour2(long seconds)
        {
            string msg = string.Format("{0:D2}:{1:D2}:{2:D2}", (seconds / 3600), (seconds % 3600) / 60, ((seconds % 3600) % 60));
            return msg;
        }

        public static string GenrateKey()
        {
            string id = "";
            change36(DateTime.Now.Ticks, ref id);
            return id;
        }

        public static void change36(long number, ref string result)
        {
            long remainder = number % 36;
            result = array[remainder] + result;
            long a = number / 36;
            if (a >= 36)
            {
                change36(a, ref result);
            }
            else if (a > 0)
            {
                result = array[a] + result;
            }
        }

        public static string GetBasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
