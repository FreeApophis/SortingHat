using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.DB
{
    class DBString
    {
        public static string ToComparison(long? value)
        {
            if (value.HasValue)
            {
                return $"= {value}";
            }

            return "IS NULL";
        }

        public static string ToSQL(long? value)
        {
            if (value.HasValue)
            {
                return value.ToString();
            }

            return "NULL";
        }
    }
}
