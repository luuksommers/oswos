using System.Collections.Generic;

namespace Oswos.Server
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if(dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key,value);
            }
        }
    }
}