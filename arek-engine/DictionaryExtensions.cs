using System;
using System.Collections.Generic;

namespace Arek.Engine
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> newValue, Func<TValue, TValue> updateValue)
        {
            if (@this.ContainsKey(key))
            {
                @this[key] = updateValue(@this[key]);
            }
            else
            {
                @this[key] = newValue();
            }
        }
    }
}