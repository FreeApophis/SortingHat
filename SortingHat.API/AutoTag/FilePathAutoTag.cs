using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class FilePathAutoTag : IAutoTag
    {
        private readonly List<string> _possibleAutoTags = new List<string>();
        public IEnumerable<string> PossibleAutoTags => _possibleAutoTags;

        private const int SelectionPart = 1;
        private const int NumberPart = 2;
        public FilePathAutoTag()
        {
            _possibleAutoTags.Add("Path.Left.0");
            _possibleAutoTags.Add("Path.Left.1");
            _possibleAutoTags.Add("Path.Left.2");
            _possibleAutoTags.Add("Path.Right.0");
            _possibleAutoTags.Add("Path.Right.1");
            _possibleAutoTags.Add("Path.Right.2");
        }

        public string HandleTag(string autoTag, string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            var pathElements = PathHelper.PathElements(fileInfo.Directory);
            var keyParts = autoTag.Split('.');
            int.TryParse(keyParts[NumberPart], out var index);

            switch (keyParts[SelectionPart])
            {
                case "Left":
                    return pathElements[pathElements.Count - index - 1];
                case "Right":
                    return pathElements[index];
                default:
                    throw new NotImplementedException();
            }
        }


    }
}
