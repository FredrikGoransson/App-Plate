using System;
using System.Collections.Generic;
using System.Linq;

namespace AppPlate.Core.Extensions
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value) ? value : null;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsInteger(this string value)
        {
            int result;
            return int.TryParse(value, out result);
        }

        public static bool IsEqualTo(this string a, string b, bool caseSensitive = true)
        {
            if (caseSensitive)
                return string.Equals(a, b, StringComparison.Ordinal);
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static int AsInt(this string value)
        {
            int result;
            int.TryParse(value, out result);
            return result;
        }
    }

}