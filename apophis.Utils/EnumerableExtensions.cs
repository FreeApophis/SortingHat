using System.Collections.Generic;
using System.Linq;

namespace apophis.Utils
{
    public static class EnumerableExtensions
    {
        extension<T>(IEnumerable<T> self)
        {
            public IEnumerable<(T item, int index)> WithIndex()
                => self.Select((item, index) => (item, index));
        }
    }
}
