using System;

namespace Id.PowershellExtensions
{
    public static class StringExtensions
    {
        public static string SubstringAfter(this string original, string value, StringComparison comparisonType)
        {
            return original.SubstringAfter(original.IndexOf(value, comparisonType), value.Length);
        }

        public static string SubstringBefore(this string original, string value, StringComparison comparisonType)
        {
            return SubstringBefore(original, original.IndexOf(value, comparisonType));
        }

        private static string SubstringAfter(this string original, int index, int length)
        {
            if (index < 0)
                return original;
            index += length;
            return original.Substring(index, original.Length - index);
        }

        private static string SubstringBefore(this string original, int index)
        {
            if (index < 0)
                return original;
            return original.Substring(0, index);
        }
    }
}