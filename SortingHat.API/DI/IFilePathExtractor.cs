using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IFilePathExtractor
    {
        IEnumerable<string> FromFilePatterns(IEnumerable<string> filePatterns, bool recursive);
    }
}
