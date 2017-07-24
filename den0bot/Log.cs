using System;
using System.IO;

namespace den0bot
{
    public static class Log
    {
        // !! FIXME: Environment.CurrentDirectory is NOT the proper way of getting app folder.
        public static void Error(object source, string text)
        {
            string sourceName = source.GetType().Name;

            if (sourceName == "String")
                sourceName = (string)source;

            string result = string.Format("({0}) [ERROR] {1}: {2}" + Environment.NewLine, DateTime.Now, sourceName, text);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(result);
            Console.ResetColor();

            File.AppendAllText(Environment.CurrentDirectory + "/log.txt", result);
        }

        public static void Info(object source, string text)
        {
            string sourceName = source.GetType().Name;

            if (sourceName == "String")
                sourceName = (string)source;

            string result = string.Format("({0}) {1}: {2}" + Environment.NewLine, DateTime.Now, sourceName, text);

            Console.Write(result);

            File.AppendAllText(Environment.CurrentDirectory + "/log.txt", result);
        }
    }
}
