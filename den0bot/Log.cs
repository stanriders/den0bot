﻿using System;
using System.IO;

namespace den0bot
{
    public static class Log
    {
        public static void Error(object source, string text)
        {
            string sourceName = source.GetType().Name;

            if (sourceName == "String")
                sourceName = (string)source;

            if (text == null)
                text = "null";

            string result = string.Format("({0}) [ERROR] {1}: {2}" + Environment.NewLine, DateTime.Now, sourceName, text);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(result);
            Console.ResetColor();

            File.AppendAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\log.txt", result);
        }

        public static void Info(object source, string text)
        {
            string sourceName = source.GetType().Name;

            if (sourceName == "String")
                sourceName = (string)source;

            if (text == null)
                text = "null";

            string result = string.Format("({0}) {1}: {2}" + Environment.NewLine, DateTime.Now, sourceName, text);

            Console.Write(result);

            File.AppendAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\log.txt", result);
        }
    }
}
