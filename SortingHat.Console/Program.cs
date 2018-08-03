
using System;
using SortingHat.API;

namespace SortingHat.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var argumentParser = new ArgumentParser(args, new CLIService());

                argumentParser.Execute();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("no such table:"))
                {
                    Console.WriteLine("Database not initialized? Run .hat init");
                }
                else
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(-1);
                }
            }
        }
    }
}
