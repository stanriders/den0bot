using System;

namespace den0bot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Bot().Start();
            Console.WriteLine("Exiting...");
            Console.Read();
        }
    }
}
