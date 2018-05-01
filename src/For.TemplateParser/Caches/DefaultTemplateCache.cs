using System.Collections.Generic;
using System.Threading;

namespace For.TemplateParser.Caches
{
    public interface ITemplateCacheProvider
    {
        bool IsExist(string key);
        object GetValue(string key);
        void Add(string key, object value);
        void Reset(string key, object value);
        void Lock();
        void Unlock();
        void RemoveCache();
    }
    internal class DefaultTemplateCacheProvider : ITemplateCacheProvider
    {
        private readonly Dictionary<string, object> _dictionaryTemplates = new Dictionary<string, object>();

        /// <summary>
        /// check cache is exist
        /// </summary>
        /// <param name="cacheEnum">which cache</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsExist(string key)
        {
            var result = false;
            result = _dictionaryTemplates.ContainsKey(key);
            return result;
        }

        /// <summary>
        /// get cache
        /// </summary>
        /// <param name="cacheEnum"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue(string key)
        {
            _dictionaryTemplates.TryGetValue(key, out object obj);
            return obj;
        }

        /// <summary>
        /// add to cache
        /// </summary>
        /// <param name="cacheEnum"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Add(string key, object value)
        {
            _dictionaryTemplates.Add(key, value);
        }

        public void Reset(string key, object value)
        {
            _dictionaryTemplates[key] = value;
        }

        /// <summary>
        /// lock cache, make thread save
        /// </summary>
        /// <param name="cacheEnum"></param>
        public void Lock()
        {
            Monitor.Enter(_dictionaryTemplates);
        }

        /// <summary>
        /// unlock cache
        /// </summary>
        public void Unlock()
        {
            Monitor.Exit(_dictionaryTemplates);
        }


        public void RemoveCache()
        {
            _dictionaryTemplates.Clear();
        }
    }
}
