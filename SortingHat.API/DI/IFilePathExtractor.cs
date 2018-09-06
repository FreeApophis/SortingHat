using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IFilePathExtractor
    {
        List<string> FromFilePatterns(IEnumerable<string> filePatterns);
    }
}
