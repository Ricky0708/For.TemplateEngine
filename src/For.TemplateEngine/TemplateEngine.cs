using System;
using For.TemplateEngine.Caches;
using For.TemplateEngine.Models;

namespace For.TemplateEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class TemplateEngine
    {
        private readonly Core _core;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public TemplateEngine(TemplateEngineConfig config = null) : this(null, config) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="config"></param>
        public TemplateEngine(ITemplateCacheProvider cache, TemplateEngineConfig config)
        {
            if (cache == null) cache = new DefaultTemplateCacheProvider();
            if (config == null) config = new TemplateEngineConfig();
            _core = new Core(cache, config);
        }

        /// <summary>
        /// 組合範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">inatance</param>
        /// <param name="cacheKey">default is typeof(T).FullName</param>
        /// <returns>template result</returns>
        public string Render<T>(T obj, string cacheKey = null)
        {
            var delg = _core.GetTemplateDelegate(cacheKey ?? typeof(T).FullName);
            if (delg is null)
            {
                throw new Exception($"can't find any registed template by {cacheKey}");
            }
            return delg.Invoke(obj) as string;
        }

        /// <summary>
        /// 動態組合範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">inatance</param>
        /// <param name="template"></param>
        /// <param name="cacheKey">default is typeof(T).FullName</param>
        /// <returns>template result</returns>
        public string DynamicRender<T>(T obj, string template, string cacheKey = null)
        {
            var recursiveCount = 0;
            while (recursiveCount < 5)
            {
                recursiveCount += 1;
                var delg = _core.GetTemplateDelegate(cacheKey ?? typeof(T).FullName);
                if (!(delg is null)) return delg.Invoke(obj) as string;
                RegisterTemplate(typeof(T), template, cacheKey);
            }
            throw new Exception("Dynamic register error");
        }

        /// <summary>
        /// Register template,cache and get the key
        /// </summary>
        /// <typeparam name="T">use in template's object type</typeparam>
        /// <param name="template">template</param>
        /// <param name="cacheKey">if is null, default will be typeof(T).FullName</param>
        /// <returns>cache key</returns>
        public string RegisterTemplate<T>(string template, string cacheKey = null)
        {
            return RegisterTemplate(typeof(T), template, cacheKey);
        }

        /// <summary>
        /// Register template, cache and get the key
        /// </summary>
        /// <param name="type">use in template's object type</param>
        /// <param name="template">template</param>
        /// <param name="cacheKey">if is null, default will be typeof(T).FullName</param>
        /// <returns>cache key</returns>
        public string RegisterTemplate(Type type, string template, string cacheKey = null)
        {
            var key = cacheKey ?? type.FullName;
            _core.RegisterTemplate(type, template, key);
            return key;
        }
    }
}
