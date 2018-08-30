using SortingHat.API.Plugin;

namespace ExifTaggerPlugin
{
    public class ExifTagger : IPlugin
    {
        public string Name => "Exif Tagger";

        public bool Execute()
        {
            System.Console.WriteLine("Exif Tagger Executed!");

            return true;
        }
    }
}
