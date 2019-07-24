using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.CliAbstractions.Formatting
{
    static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        {
            return self.Select((item, index) => (item, index));
        }

        public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (T item in source)
            {
                action(item);
            }
        }
    }


}
