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

            WriteStringToFile(text, Environment.CurrentDirectory + "/log.txt");
        }

        public static void Info(object source, string text)
        {
            string sourceName = source.GetType().Name;

            if (sourceName == "String")
                sourceName = (string)source;

            string result = sourceName + ": " + text + Environment.NewLine;

            Console.Write(result);

            WriteStringToFile(text, Environment.CurrentDirectory + "/log.txt");
        }

        public static void WriteStringToFile(string text, string path, bool encoding = false)
        {
            try
            {
                StreamWriter fs = new StreamWriter(path, true, (!encoding) ? System.Text.Encoding.GetEncoding(1251) : System.Text.Encoding.GetEncoding(65001));
                fs.Write(text + "\n");
                fs.Close();
            }
            catch (Exception e) { Console.WriteLine("WriteStringToFile: " + e.Message);  }
        }
    }
}
