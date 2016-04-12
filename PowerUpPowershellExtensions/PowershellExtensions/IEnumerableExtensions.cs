using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Id.PowershellExtensions
{
    public static class IEnumerableExtensions
    {
        public static string Aggregate<T>(this IEnumerable<T> values)
        {
            return values.Aggregate(", ");
        }

        public static string Aggregate<T>(this IEnumerable<T> values, string separator)
        {
            return values.Aggregate(
                String.Empty,
                (currentOutput, value) =>
                    (!string.IsNullOrEmpty(currentOutput))
                        ? String.Format("{0}{1}{2}", currentOutput, separator.ToString(CultureInfo.InvariantCulture),
                            value.ToString())
                        : value.ToString()
                );
        }

        public static bool NullOrEmpty<T>(this IEnumerable<T> values)
        {
            return values == null || !values.Any();
        }
    }
}