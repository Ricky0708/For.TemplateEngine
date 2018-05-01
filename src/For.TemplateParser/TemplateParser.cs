using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        /// 組合物件與範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public string BuildTemplate<T>(T obj, string template)
        {
            var n = _core.BuildTemplateInDelegate<T>(template);
            return n.Invoke(obj) as string;
        }
        public string BuildTemplateInQue<T>(T obj, string template)
        {
            var n = _core.BuildTemplateInQue(typeof(T), template);
            var templateQue = new Queue<NodeModel>(n);
            var sb = new StringBuilder();
            {
                var item = templateQue.Dequeue();
                sb.Append(item.Type == NodeType.String ? item.NodeStringValue : item.NodeDelegateValue(obj));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 組合物件與範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string BuildTemplate<T>(T obj, Func<T, string> func)
        {
            return func(obj);
        }
        /// <summary>
        /// 清除所有快取
        /// </summary>
        public void ClearCaches()
        {
            _core.ClearCache();
        }
    }
}
