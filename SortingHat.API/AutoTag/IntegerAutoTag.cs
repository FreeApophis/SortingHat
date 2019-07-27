using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Funcky.Extensions;

namespace SortingHat.API.AutoTag
{
    public abstract class IntegerAutoTag : IAutoTag
    {

        public abstract string AutoTagKey { get; }
        public abstract string Description { get; }
        public string HumanReadableAutoTagsKey =>
            AutoTagKey.Replace("<>", "<Integer> Can be any valid integer number: 0,1,2...");

        public string? HandleTag(FileInfo file, string? tagMatch)
        {
            if (tagMatch is { } match)
            {
                return match.TryParseInt().Match(
                    none: null as string,
                    some: integer => HandleTag(file, integer)
                );

            }

            return null;
        }

        protected abstract string? HandleTag(FileInfo file, int index);

        public string? FindMatch(string value)
        {
            var findDigits = new Regex(AutoTagKey.Replace("<>", @"(\d+)"));


            var match = findDigits.Match(value);

            return match.Success
                ? match.Groups[1].Value
                : null;
        }
    }
}
