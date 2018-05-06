using System.Collections.Generic;
using System.Threading;

namespace For.TemplateEngine.Caches
{
    public interface ITemplateCacheProvider
    {
        /// <summary>
        /// check cache is exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsExist(string key);
        /// <summary>
        /// get cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetValue(string key);
        /// <summary>
        /// add to cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void Add(string key, object value);
        /// <summary>
        /// reset value by key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Reset(string key, object value);
        /// <summary>
        /// lock cache, make thread save
        /// </summary>
        void Lock();
        /// <summary>
        /// unlock cache
        /// </summary>
        void Unlock();
        void RemoveCache();
    }
    internal class DefaultTemplateCacheProvider : ITemplateCacheProvider
    {
        private readonly Dictionary<string, object> _dictionaryTemplates = new Dictionary<string, object>();

        /// <summary>
        /// check cache is exist
        /// </summary>
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
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Add(string key, object value)
        {
            _dictionaryTemplates.Add(key, value);
        }

        /// <summary>
        /// reset value by key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Reset(string key, object value)
        {
            _dictionaryTemplates[key] = value;
        }

        /// <summary>
        /// lock cache, make thread save
        /// </summary>
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

        /// <summary>
        /// remove all cache
        /// </summary>
        public void RemoveCache()
        {
            _dictionaryTemplates.Clear();
        }
    }
}
