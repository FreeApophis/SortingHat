using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace ExifTaggerPlugin
{
    class ExifTaggerCommand : ICommand
    {
        private IComponentContext _container;

        public ExifTaggerCommand(IComponentContext container)
        {
            _container = container;
        }

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
            var db = _container.Resolve<IDatabase>();
            var hashService = _container.Resolve<IHashService>();
            var tagParser = _container.Resolve<ITagParser>();
            FilePathExtractor filePathExtractor = new FilePathExtractor(arguments);

            foreach (var filePath in filePathExtractor.FilePaths)
            {
                IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(filePath);

                var file = new SortingHat.API.Models.File(db, hashService);
                //var tag = new SortingHat.API.Models.Tag(db, GetCameraModel(directories), new SortingHat.API.Models.Tag(db, "Camera"));
                var tag = TagFromDate(db, GetTakenDateTime(directories));
                file.Path = filePath;
                file.LoadByPath();
                if (tag != null)
                {
                    file.Tag(tag).Wait();
                }

                //IterateAll(directories);

                Console.WriteLine($"Tag file: {file.Path} with Tag {tag.FullName}");
                //Console.WriteLine(GetCameraModel(directories));
                //Console.WriteLine(GetTakenDateTime(directories));
            }


            return true;
        }

        private static SortingHat.API.Models.Tag TagFromDate(IDatabase db, DateTime? taken)
        {
            if (taken.HasValue)
            {
                var root = new SortingHat.API.Models.Tag(db, "Taken");
                var year = new SortingHat.API.Models.Tag(db, taken.Value.Year.ToString(), root);
                var month = new SortingHat.API.Models.Tag(db, taken.Value.Month.ToString(), year);
                var day = new SortingHat.API.Models.Tag(db, taken.Value.Day.ToString(), month);

                return day;
            }

            return null;
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
