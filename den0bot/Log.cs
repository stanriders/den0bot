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

            string result = "(" + DateTime.Now + ") " + sourceName + ": " + text + Environment.NewLine;

            Console.Write(result);

            File.AppendAllText(Environment.CurrentDirectory + "/log.txt", result);
        }

        public static void Info(object source, string text)
        {
            string sourceName = source.GetType().Name;

            if (sourceName == "String")
                sourceName = (string)source;

            string result = sourceName + ": " + text + Environment.NewLine;

            Console.Write(result);

            File.AppendAllText(Environment.CurrentDirectory + "/log.txt", result);
        }
    }
}
