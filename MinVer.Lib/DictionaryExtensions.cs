using System;
using System.Collections.Generic;

namespace MinVer.Lib
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
            where TKey : notnull
            where TValue : notnull
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                dictionary.Add(key, value = valueFactory());
            }

            return value;
        }
    }
}
