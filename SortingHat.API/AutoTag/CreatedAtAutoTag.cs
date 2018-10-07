using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class CreatedAtAutoTag : IAutoTag
    {
        private readonly List<IDateTagPart> _dateParts = new List<IDateTagPart> { new YearPart(), new MonthPart(), new DayPart(), new HourPart() };
        private const string TagName = "CreatedAt";

        private readonly List<string> _possibleAutoTags = new List<string>();
        public IEnumerable<string> PossibleAutoTags => _possibleAutoTags;

        public CreatedAtAutoTag()
        {
            _possibleAutoTags.InsertRange(0, _dateParts.Select(TagNameWithDatePart));
        }

        private string TagNameWithDatePart(IDateTagPart datePart)
        {
            return $"{TagName}.{datePart.Key}";
        }

        public string HandleTag(string autoTag, string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            return _dateParts.First(datePart => TagNameWithDatePart(datePart) == autoTag).Select(fileInfo.CreationTime);
        }
    }
}
