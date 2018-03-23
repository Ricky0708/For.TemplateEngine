using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace For.TemplateParser
{
    internal static class Caches
    {

        private static Dictionary<string, object> dictionaryGetPropertyValue = new Dictionary<string, object>();

        /// <summary>
        /// check cache is exist
        /// </summary>
        /// <param name="cacheEnum">which cache</param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool IsExist(CacheType cacheEnum, string key)
        {
            bool result = false;
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    result = dictionaryGetPropertyValue.ContainsKey(key);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// get cache
        /// </summary>
        /// <param name="cacheEnum"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static object GetValue(CacheType cacheEnum, string key)
        {

            object obj = null;
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    obj = dictionaryGetPropertyValue[key];
                    break;
                default:
                    break;
            }
            return obj;
        }

        /// <summary>
        /// add to cache
        /// </summary>
        /// <param name="cacheEnum"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static object Add(CacheType cacheEnum, string key, object value)
        {
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    dictionaryGetPropertyValue.Add(key, value);
                    break;
                default:
                    break;
            }
            return value;
        }

        /// <summary>
        /// lock cache, make thread save
        /// </summary>
        /// <param name="cacheEnum"></param>
        internal static void Lock(CacheType cacheEnum)
        {
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    Monitor.Enter(dictionaryGetPropertyValue);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// unlock cache
        /// </summary>
        /// <param name="cacheEnum"></param>
        internal static void Unlock(CacheType cacheEnum)
        {
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    Monitor.Exit(dictionaryGetPropertyValue);
                    break;
                default:
                    break;
            }
        }
    }
}
