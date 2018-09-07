using System;
using System.Collections.Generic;
using Autofac;
using SortingHat.API.DI;

namespace SortingHat.Plugin.Exif
{
    class ExifCommand : ICommand
    {
        private IComponentContext _container;

        public ExifCommand(IComponentContext container)
        {
            _container = container;
        }



        private static API.Models.Tag TagFromDate(IDatabase db, DateTime? taken)
        {
            if (taken.HasValue)
            {
                var root = new API.Models.Tag(db, "Taken");
                var year = new API.Models.Tag(db, taken.Value.Year.ToString(), root);
                var month = new API.Models.Tag(db, taken.Value.Month.ToString(), year);
                var day = new API.Models.Tag(db, taken.Value.Day.ToString(), month);

                return day;
            }

            return null;
        }

        public bool Execute(IEnumerable<string> arguments)
        {
            return true;
        }

        public string LongCommand => "exif";
        public string ShortCommand => null;
        public string ShortHelp => "Reads exif information from files.";

        public CommandGrouping CommandGrouping => CommandGrouping.General;
    }
}
