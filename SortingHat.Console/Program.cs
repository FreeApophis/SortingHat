
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
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }
    }
}
