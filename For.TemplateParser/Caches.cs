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
        private static Object lockObject = new object();
        private static Dictionary<string, object> dictionaryGetPropertyValue = new Dictionary<string, object>();
        private static Dictionary<string, object> dictionaryUsedPropertyName = new Dictionary<string, object>();
        private static Dictionary<string, object> dictionaryPropertys = new Dictionary<string, object>();
        private static Dictionary<string, object> dictionaryTemplates = new Dictionary<string, object>();

        /// <summary>
        /// check cache is exist
        /// </summary>
        /// <param name="cacheEnum">which cache</param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool IsExist(CacheType cacheEnum, string key)
        {
            var n = lockObject;
            bool result = false;
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    result = dictionaryGetPropertyValue.ContainsKey(key);
                    break;
                case CacheType.UsedPropertyName:
                    result = dictionaryUsedPropertyName.ContainsKey(key);
                    break;
                case CacheType.Propertys:
                    result = dictionaryPropertys.ContainsKey(key);
                    break;
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
            var n = lockObject;
            object obj = null;
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    obj = dictionaryGetPropertyValue[key];
                    break;
                case CacheType.UsedPropertyName:
                    obj = dictionaryUsedPropertyName[key];
                    break;
                case CacheType.Propertys:
                    obj = dictionaryPropertys[key];
                    break;
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
            var n = lockObject;
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    dictionaryGetPropertyValue.Add(key, value);
                    break;
                case CacheType.UsedPropertyName:
                    dictionaryUsedPropertyName.Add(key, value);
                    break;
                case CacheType.Propertys:
                    dictionaryPropertys.Add(key, value);
                    break;
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
        internal static void Lock(CacheType cacheEnum)
        {
            Monitor.Enter(lockObject);
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    Monitor.Enter(dictionaryGetPropertyValue);
                    break;
                case CacheType.UsedPropertyName:
                    Monitor.Enter(dictionaryUsedPropertyName);
                    break;
                case CacheType.Propertys:
                    Monitor.Enter(dictionaryPropertys);
                    break;
                case CacheType.Template:
                    Monitor.Enter(dictionaryTemplates);
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
                case CacheType.UsedPropertyName:
                    Monitor.Exit(dictionaryUsedPropertyName);
                    break;
                case CacheType.Propertys:
                    Monitor.Exit(dictionaryPropertys);
                    break;
                case CacheType.Template:
                    Monitor.Exit(dictionaryTemplates);
                    break;
                default:
                    break;
            }
            Monitor.Exit(lockObject);
        }

        internal static void RemoveCache(CacheType cacheEnum)
        {
            switch (cacheEnum)
            {
                case CacheType.GetPropertyValue:
                    dictionaryGetPropertyValue.Clear();
                    break;
                case CacheType.UsedPropertyName:
                    dictionaryUsedPropertyName.Clear();
                    break;
                case CacheType.Propertys:
                    dictionaryPropertys.Clear();
                    break;
                case CacheType.Template:
                    dictionaryTemplates.Clear();
                    break;
                default:
                    break;
            }
            //switch (cacheEnum)
            //{
            //    case CacheType.GetPropertyValue:
            //        Lock(CacheType.GetPropertyValue);
            //        dictionaryGetPropertyValue.Clear();
            //        Unlock(CacheType.GetPropertyValue);
            //        break;
            //    case CacheType.UsedPropertyName:
            //        Lock(CacheType.UsedPropertyName);
            //        dictionaryUsedPropertyName.Clear();
            //        Unlock(CacheType.UsedPropertyName);
            //        break;
            //    case CacheType.Propertys:
            //        Lock(CacheType.Propertys);
            //        dictionaryPropertys.Clear();
            //        Unlock(CacheType.Propertys);
            //        break;
            //    case CacheType.Template:
            //        Lock(CacheType.Template);
            //        dictionaryTemplates.Clear();
            //        Unlock(CacheType.Template);
            //        break;
            //    default:
            //        break;
            //}
        }
    }
}
