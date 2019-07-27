using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IProjects
    {
        IEnumerable<string> GetProjects();
    }
}
