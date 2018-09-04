﻿// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace den0bot
{
    public static class Extensions // todo: remove
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
    }
}
