using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class CreatedAtAutoTag : IAutoTag
    {
        private readonly List<IDateTagPart> _dateParts = new List<IDateTagPart>() { new YearPart(), new MonthPart(), new DayPart() };
        private readonly string _tagName = "CreatedAt";

        public CreatedAtAutoTag()
        {
            _possibleAutoTags.InsertRange(0, _dateParts.Select(datePart => TagName(datePart)));
        }

        private string TagName(IDateTagPart datePart)
        {
            return $"{_tagName}.{datePart.Key}";
        }

        private List<string> _possibleAutoTags = new List<string>();
        public IEnumerable<string> PossibleAutoTags => _possibleAutoTags;

        public string HandleTag(string autoTag, string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            return _dateParts.Where(datePart => TagName(datePart) == autoTag).First().Select(fileInfo.CreationTime);
        }
    }
}
