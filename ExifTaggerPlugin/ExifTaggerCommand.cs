using System;
using System.Collections.Generic;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SortingHat.API.DI;

namespace ExifTaggerPlugin
{
    class ExifTaggerCommand : ICommand
    {
        string GetCameraModel(IEnumerable<Directory> directories)
        {
            // obtain the Exif SubIFD directory
            var directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

            if (directory == null)
                return null;

            // query the tag's value
            return directory.GetString(ExifDirectoryBase.TagModel);
        }

        DateTime? GetTakenDateTime(IEnumerable<Directory> directories)
        {
            // obtain the Exif SubIFD directory
            var directory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if (directory == null)
                return null;

            // query the tag's value
            if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTime))
                return dateTime;

            return null;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata("C:\\Users\\Thoma\\Pictures\\5D MKII\\GM1B3543.jpg");
            //IterateAll(directories);

            Console.WriteLine("---");
            Console.WriteLine(GetCameraModel(directories));
            Console.WriteLine(GetTakenDateTime(directories));

            return true;
        }

        private static void IterateAll(IEnumerable<Directory> directories)
        {
            foreach (var directory in directories)
            {
                Console.WriteLine($"Directory: {directory.Name}");
                foreach (var tag in directory.Tags)
                {
                    Console.WriteLine($" ({tag.GetType()}) {tag.Name} = {tag.Description}");
                }
            }
        }

        public string LongCommand => "exif-auto-tag";
        public string ShortCommand => null;
        public string ShortHelp => "Tag files automatically according to their exif tags.";
    }
}
