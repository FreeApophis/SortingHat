
using System;
using OptParse;
using SortingHat.API;

namespace SortingHat.CLI
{
    class Program
    {
        private const string HelpCommand = "help";
        private const string InitCommand = "init";

        static void Main(string[] args)
        {
            try
            {
                var options = ParseCommandLine(args);

                if (options.GetOption(HelpCommand) != null)
                {
                    Console.WriteLine(options.Help);
                }

                if (options.GetOption(InitCommand) != null)
                {
                    DB.Init();
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }

        private static OptParser ParseCommandLine(string[] args)
        {
            OptParser options = OptParser.CreateOptionParser("SortingHat.CLI", "Add tags to files")
                .AddOption('f', "files", OptionType.Optional, "", "Just files")
                .AddOption('d', "directories", OptionType.Optional, "", "Just directories")
                .AddOption('h', HelpCommand, OptionType.Optional, "", "Show this help")
                .AddOption('i', InitCommand, OptionType.Optional, "", "Initialize Database")
                .AddPathOrExpression("path", OptionType.Optional, ".", "Directory to listing");

            options.ParseArguments(args);
            return options;
        }
    }
}
