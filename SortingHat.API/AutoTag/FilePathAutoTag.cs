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

        public FilePathAutoTag()
        {
            _possibleAutoTags.Add("Path.Recursive");
            _possibleAutoTags.Add("Path.Left");
            _possibleAutoTags.Add("Path.Right");
        }

        public string HandleTag(string autoTag, string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            var pathElements = PathHelper.PathElements(fileInfo.Directory);

            return null;
        }


    }
}
