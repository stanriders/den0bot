using System;

namespace den0bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Info("", "________________");
            new Bot().Start();
            Log.Info("", "Exiting...");
            Console.Read();
        }
    }
}
