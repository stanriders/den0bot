// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace den0bot.Util
{
    public static class Extensions
    {
        public static string FilterHTML(this string value)
        {
            var step1 = value.Replace("<br>", Environment.NewLine);

            var step2 = Regex.Replace(step1, @"<[^>]+>", "").Trim();

            return Regex.Replace(step2, @"\s{2,}", " ")
                        .Replace("&gt;", ">")
                        .Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"")
                        .Replace("&#47;", "/");
        }

        public static string FilterToHTML(this string value)
        {
            return value.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;");
        }

        public static string InnerMessageIfAny(this Exception value)
        {
            return value.InnerException?.Message ?? value.Message;
        }

        public static TKey GetKeyByValue<TKey,TValue>(this Dictionary<TKey, TValue> dic, TValue value) where TValue : class
        {
            return dic.FirstOrDefault(x => x.Value == value).Key;
        }

        public static string FN2 (this double value)
        {
            return value.ToString("N2");
        }

		public static Osu.Mods ConvertToMods(this string mods)
		{
			Osu.Mods result = Osu.Mods.None;
			if (Enum.TryParse(mods, true, out result) || string.IsNullOrEmpty(mods))
				return result;
			else
			{
				StringBuilder builder = new StringBuilder(mods.Length * 2);
				bool secondChar = false;
				foreach (char c in mods)
				{
					builder.Append(c);
					if (secondChar)
					{
						builder.Append(',');
						builder.Append(' ');
					}
					secondChar = !secondChar;
				}
				builder.Remove(builder.Length - 2, 2);
				Enum.TryParse(builder.ToString(), true, out result);
				return result;
			}
		}
    }
}
