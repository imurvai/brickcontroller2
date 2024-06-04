using System.Collections.Generic;
using System.Linq;

namespace BrickController2.Helpers
{
    public static class DictionaryExtensions
    {
        public static TKey[] GetKeyArray<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey: notnull
        {
            return dictionary.Keys.Select(x => x).ToArray();
        }
    }
}
