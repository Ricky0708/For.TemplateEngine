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
        public TemplateParser()
        {
            _core = new Core();
        }

        public TemplateParser(ITemplateCacheProvider cache)
        {
            _core = new Core(cache);
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
            var que = _core.BuildTemplate(typeof(T), template);
            var sb = new StringBuilder();
            return ProcessQue(obj, que);
        }
        private string ProcessQue<T>(T obj, Queue<NodeModel> que)
        {
            var templateQue = new Queue<NodeModel>(que);
            var sb = new StringBuilder();
            while (templateQue.Count > 0)
            {
                var item = templateQue.Dequeue();
                //var result = item.Type == NodeType.String ? item.NodeStringValue : item.NodeDelegateValue(obj);
                object result = "";
                switch (item.Type)
                {
                    case NodeType.String:
                        result = item.NodeStringValue;
                        break;
                    case NodeType.Collection:
                        result = ProcessQue(obj, item.SubQue);
                        break;
                    case NodeType.Delegate:
                        result = item.NodeDelegateValue(obj);
                        break;
                    default:
                        break;
                }
                sb.Append(result);
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
