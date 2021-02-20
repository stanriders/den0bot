// den0bot (c) StanR 2021 - MIT License
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace den0bot.Util
{
	public static class Log
	{
		public static void Error(string text, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
		{
			string result = $"({DateTime.Now}) [ERROR] {NameOfCallingClass()} ({Path.GetFileName(file)}:{line}): {text ?? "null"}{Environment.NewLine}";

			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write(result);
			Console.ResetColor();

			File.AppendAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
		}

		public static void Info(string text)
		{
			string result = $"({DateTime.Now}) {NameOfCallingClass()}: {text ?? "null"}{Environment.NewLine}";

			Console.Write(result);
			File.AppendAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
		}

		public static void Debug(string text, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
		{
#if DEBUG
			string result = $"({DateTime.Now}) {NameOfCallingClass()}::{caller} ({Path.GetFileName(file)}:{line}): {text ?? "null"}{Environment.NewLine}";

			Console.Write(result);
			File.AppendAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
#endif
		}

		public static void Debug(string text, Type callingClass, [CallerMemberName] string caller = "")
		{
#if DEBUG
			string result = $"({DateTime.Now}) {callingClass.Name}::{caller}: {text ?? "null"}{Environment.NewLine}";

			Console.Write(result);
			File.AppendAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "log.txt", result);
#endif
		}

		private static string NameOfCallingClass()
		{
			return new StackFrame(2, false).GetMethod()?.DeclaringType?.Name;
		}
	}
}
