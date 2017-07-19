using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace den0bot
{
    public static class Extensions
    {
        public static string GetUsername(this Enum value)
            => value.GetType().GetField(value.ToString())
            .GetCustomAttribute<UserAttribute>()?.Name ?? value.ToString();

        public static uint GetUserID(this Enum value)
            => value.GetType().GetField(value.ToString()).GetCustomAttribute<UserAttribute>()?.ID ?? uint.Parse(value.ToString());

        public static string FilterHTML(string value)
        {
            string result;

            var step0 = value.Replace("<br>", "\n");

            var step1 = Regex.Replace(step0, @"<[^>]+>", "").Trim();

            var step2 = Regex.Replace(step1, @"\s{2,}", " ");

            var step3 = step2.Replace("&gt;", ">");

            var step4 = step3.Replace("&nbsp;", " ");

            var step5 = step4.Replace("&quot;", "\"");

            var step6 = step5.Replace("&#47;", "/");

            result = step6;

            return result;
        }

        public static string FilterToHTML(string value)
        {
            string result;

            var step1 = value.Replace("&", "&amp;");

            var step2 = step1.Replace("<", "&lt;");

            var step3 = step2.Replace(">", "&gt;" );

            var step4 = step3.Replace("\"", "&quot;");

            result = step4;

            return result;
        }
    }
}
