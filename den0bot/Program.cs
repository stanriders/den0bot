using System;

namespace den0bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Info("", Environment.NewLine + "Starting at " + DateTime.Now);
            new Bot().Start();
            Log.Info("", "Exiting...");
            Console.Read();
        }
    }
}
