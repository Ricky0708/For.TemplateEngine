using System;
using For.TemplateParser.Caches;
using For.TemplateParser.Models;

namespace For.TemplateParser
{
    public class TemplateParser
    {
        private readonly Core _core;
        public TemplateParser(TemplateParserConfig config = null)
        {
            if (config == null) config = new TemplateParserConfig();
            _core = new Core(config);
        }

        public TemplateParser(ITemplateCacheProvider cache, TemplateParserConfig config = null)
        {
            if (config == null) config = new TemplateParserConfig();
            _core = new Core(cache, config);
        }

        /// <summary>
        /// 組合範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">inatance</param>
        /// <param name="cacheKey">default is typeof(T).FullName</param>
        /// <returns>template result</returns>
        public string BuildTemplate<T>(T obj, string cacheKey = null)
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
        public string DynamicBuildTemplate<T>(T obj, string template, string cacheKey = null)
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
        /// <typeparam name="T"></typeparam>
        /// <param name="template">inatance</param>
        /// <param name="cacheKey">default is typeof(T).FullName</param>
        /// <returns>cache key</returns>
        public string RegisterTemplate<T>(string template, string cacheKey = null)
        {
            return RegisterTemplate(typeof(T), template, cacheKey);
        }


        /// <summary>
        /// Register template, cache and get the key
        /// </summary>
        /// <param name="type"></param>
        /// <param name="template">inatance</param>
        /// <param name="cacheKey">default is typeof(T).FullName</param>
        /// <returns>cache key</returns>
        public string RegisterTemplate(Type type, string template, string cacheKey = null)
        {
            var key = cacheKey ?? type.FullName;
            _core.RegisterTemplate(type, template, key);
            return key;
        }
    }
}
