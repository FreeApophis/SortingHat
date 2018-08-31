using MetadataExtractor;
using SortingHat.API.Plugin;
using System;
using System.Collections.Generic;

namespace ExifTaggerPlugin
{
    public class ExifTagger : IPlugin
    {
        public string Name => "Exif Tagger";

        public bool Execute()
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata("C:\\Users\\Thoma\\Pictures\\5D MKII\\GM1B3543.jpg");
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                }
            }

            return true;
        }
    }
}
