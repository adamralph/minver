namespace MinVer.Lib
{
    using System;
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                dictionary.Add(key, value = valueFactory());
            }

            return value;
        }
    }
}
