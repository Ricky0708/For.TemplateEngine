using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace For.TemplateParser
{
    internal static class Caches
    {
        private static Dictionary<string, object> dictionaryTemplates = new Dictionary<string, object>();

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
                case CacheType.Template:
                    result = dictionaryTemplates.ContainsKey(key);
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
                case CacheType.Template:
                    obj = dictionaryTemplates[key];
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
                case CacheType.Template:
                    dictionaryTemplates.Add(key, value);
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
        internal static void Lock()
        {
            Monitor.Enter(dictionaryTemplates);
        }

        /// <summary>
        /// unlock cache
        /// </summary>
        /// <param name="cacheEnum"></param>
        internal static void Unlock()
        {
            Monitor.Exit(dictionaryTemplates);
        }

        internal static void RemoveCache(CacheType cacheEnum)
        {
            switch (cacheEnum)
            {
                case CacheType.Template:
                    dictionaryTemplates.Clear();
                    break;
                default:
                    break;
            }
        }
    }
}
