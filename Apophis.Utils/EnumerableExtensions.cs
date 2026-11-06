using System.Collections.Generic;
using System.Linq;

namespace Apophis.Utils;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> self)
    {
        public IEnumerable<(T Item, int Index)> WithIndex()
            => self.Select((item, index) => (item, index));
    }
}
