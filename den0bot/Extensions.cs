
using System.Text.RegularExpressions;

namespace den0bot
{
    public static class Extensions // todo: remove
    {
        public static string FilterHTML(string value)
        {
            string result;

            var step0 = value.Replace("<br>", "\n");

            var step1 = Regex.Replace(step0, @"<[^>]+>", "").Trim();

            result = Regex.Replace(step1, @"\s{2,}", " ")
                            .Replace("&gt;", ">")
                            .Replace("&nbsp;", " ")
                            .Replace("&quot;", "\"")
                            .Replace("&#47;", "/");

            return result;
        }

        public static string FilterToHTML(string value)
        {
            string result = value.Replace("&", "&amp;")
                                .Replace("<", "&lt;")
                                .Replace(">", "&gt;")
                                .Replace("\"", "&quot;");

            return result;
        }
    }
}
