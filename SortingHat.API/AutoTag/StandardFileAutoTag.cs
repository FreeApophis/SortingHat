using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SortingHat.API.DI;

namespace SortingHat.API
{
    [UsedImplicitly]
    class DefaultFileAutoTag : IAutoTag
    {
        public IEnumerable<string> PossibleAutoTags => throw new System.NotImplementedException();

        public string HandleTag(string autoTag, string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            switch(autoTag)
            {
            }

            return null;
        }
    }
}
