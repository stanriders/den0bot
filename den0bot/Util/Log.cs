// den0bot (c) StanR 2018 - MIT License
using System;
using System.IO;

namespace den0bot.Util
{
	public static class Log
	{
		public static void Error(object source, string text)
		{
			string sourceName = source.GetType().Name;

			if (sourceName == "String")
				sourceName = (string)source;

			string result = $"({DateTime.Now}) [ERROR] {sourceName}: {text ?? "null"}{Environment.NewLine}";

			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(result);
			Console.ResetColor();

			File.AppendAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
		}

		public static void Info(object source, string text)
		{
			string sourceName = source.GetType().Name;

			if (sourceName == "String")
				sourceName = (string)source;

			string result = $"({DateTime.Now}) {sourceName}: {text ?? "null"}{Environment.NewLine}";

			Console.Write(result);

			File.AppendAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
		}

		public static void Debug(object source, string text)
		{
#if DEBUG
			string sourceName = source.GetType().Name;

			if (sourceName == "String")
				sourceName = (string)source;

			string result = $"({DateTime.Now}) {sourceName}: {text ?? "null"}{Environment.NewLine}";

			Console.Write(result);

			File.AppendAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
#endif
		}

		public static void IRC(string sender, string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			string result = string.Format("({0}) {1}: {2}" + Environment.NewLine, DateTime.Now, sender, text);

			File.AppendAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.PathSeparator + "IRC.txt", result);
		}
	}
}
