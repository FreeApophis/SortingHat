using System.IO;
using System.Text.RegularExpressions;

namespace SortingHat.API.AutoTag
{
    public abstract class IntegerAutoTag : IAutoTag
    {

        public abstract string AutoTagKey { get; }
        public abstract string Description { get; }
        public string HumanReadableAutoTagsKey =>
            AutoTagKey.Replace("<>", "<Integer> Can be any valid integer number: 0,1,2...");

        public string? HandleTag(FileInfo file, string tagMatch)
        {
            return int.TryParse(tagMatch, out var integer)
                ? HandleTag(file, integer)
                : null;
        }

        protected abstract string? HandleTag(FileInfo file, int index);

        public string FindMatch(string value)
        {
            var findDigits = new Regex(AutoTagKey.Replace("<>", @"(\d+)"));


            var match = findDigits.Match(value);

            if (!match.Success)
            {
                return null;
            }

            return match.Groups[1].Value;
        }
    }
}
