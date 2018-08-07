
using System;
using SortingHat.API.Parser;

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
            catch (ParseException e)
            {
                Console.WriteLine("Parser is not happy with your input, maybe find a ravenclaw...");
                Console.WriteLine(e.Message);
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
