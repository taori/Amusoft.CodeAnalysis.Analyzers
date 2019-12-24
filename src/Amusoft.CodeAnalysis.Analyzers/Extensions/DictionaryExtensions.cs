using System;
using System.Collections.Generic;

namespace Amusoft.CodeAnalysis.Analyzers.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets value from dictionary or value for key
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrInitialize<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, Func<TKey, TValue> value)
        {
            if (source.ContainsKey(key))
            {
                return source[key];
            }
            else
            {
                source.Add(key, value(key));
                return source[key];
            }
        }
    }
}