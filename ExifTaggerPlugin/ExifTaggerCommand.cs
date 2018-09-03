using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SortingHat.API.DI;

namespace ExifTaggerPlugin
{
    class ExifTaggerCommand : ICommand
    {
        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("Tagging started...");
            Thread.Sleep(1000);
            Console.WriteLine("Tagging ended...");

            return true;
        }

        public string LongCommand => "exif-auto-tag";
        public string ShortCommand => null;
        public string ShortHelp => "Tag files automatically according to their exif tags.";
    }
}
