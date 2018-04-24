using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using static For.TemplateParser.NodeModel;

namespace For.TemplateParser
{
    internal class Core
    {
        private readonly ITemplateCache _templateCache;

        /// <summary>
        /// 由範本中抽取特殊標記的pattern
        /// </summary>
        private static readonly Regex _regex = new Regex("({.\\w*})");

        public Core()
        {
            _templateCache = new DefaultTemplateCache();
        }
        public Core(ITemplateCache templateCache)
        {
            _templateCache = templateCache;
        }

        /// <summary>
        /// 建立範本的委派及cache
        /// </summary>
        /// <param name="type"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        internal Queue<NodeModel> BuildTemplate(Type type, string template)
        {

            if (!_templateCache.IsExist(template))
            {
                _templateCache.Lock();
                if (!_templateCache.IsExist(template))
                {
                    _templateCache.Add(template, _BuildTemplate(type, template));
                }
                _templateCache.Unlock();
            }

            var result = _templateCache.GetValue(template) as Queue<NodeModel>;
            if (result is null)
            {
                result = BuildTemplate(type, template);

            }
            return result;
        }

        internal void ClearCache()
        {
            _templateCache.Lock();
            _templateCache.RemoveCache();
            _templateCache.Unlock();
        }

        private static Queue<NodeModel> _BuildTemplate(Type type, string template)
        {
            var array = _regex.Split(template);
            var que = new Queue<NodeModel>();
            foreach (var item in array)
            {
                if (item.StartsWith("{."))
                {
                    que.Enqueue(new NodeModel()
                    {
                        Type = NodeType.Property,
                        NodeDelegateValue = _BuildGetPropertyMethod(type, item.Replace("{.", "").Replace("}", "")),
                    });
                }
                else
                {
                    que.Enqueue(new NodeModel()
                    {
                        Type = NodeType.String,
                        NodeStringValue = item,
                    });
                }
            }
            return que;
        }

        private static delgGetProperty _BuildGetPropertyMethod(Type type, string prop)
        {
            var targetExp = Expression.Parameter(typeof(object), "target");
            var propertyExp = Expression.Property(Expression.Convert(targetExp, type), prop);

            var lambdax = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(propertyExp, typeof(object)), targetExp);
            var delg = (delgGetProperty)lambdax.Compile();
            return delg;
        }
    }
}
