using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using For.TemplateParser.Caches;
using For.TemplateParser.Models;
using static For.TemplateParser.Models.NodeModel;

namespace For.TemplateParser
{
    internal class Core
    {
        private readonly ITemplateCacheProvider _templateCache;

        /// <summary>
        /// 由範本中抽取特殊標記的pattern
        /// </summary>
        //private static Regex _regexProperty => new Regex(@"({\..*?})");
        private static Regex _regexProperty => new Regex(@"(?<=[^#])({\..*?})(?<=[^#])|(^{\..*?})");

        private static Regex _regexList => new Regex(@"(\[\#.*?(\#])+)");

        public Core()
        {
            _templateCache = new DefaultTemplateCacheProvider();
        }
        public Core(ITemplateCacheProvider templateCache)
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
            var forPropertyArray = _regexProperty.Split(template);
            var que = new Queue<NodeModel>();
            foreach (var item in forPropertyArray)
            {
                if (item.StartsWith("{."))
                {
                    que.Enqueue(new NodeModel()
                    {
                        Type = NodeType.Delegate,
                        NodeDelegateValue = _BuildGetPropertyMethod(type, item.Replace("{.", "").Replace("}", "").Split('.')),
                    });
                }
                else if (item.IndexOf("[#") > -1)
                {
                    var temp = item;
                    int first = temp.IndexOf("[#");
                    temp = temp.Remove(first, 2);
                    int last = temp.IndexOf("#]");
                    temp = temp.Remove(last, 2);
                    que.Enqueue(new NodeModel()
                    {
                        Type = NodeType.Collection,
                        SubQue = _BuildTemplate(type, temp),
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

        private static delgGetProperty _BuildGetPropertyMethod(Type type, params string[] props)
        {
            delgGetProperty dlgResult = null;
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            Expression memberExp = Expression.Convert(targetExp, type);
            LambdaExpression lambdaExp;
            for (int i = 0; i < props.Length; i++)
            {
                memberExp = Expression.Property(memberExp, props[i]);
            }
            lambdaExp = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(memberExp, typeof(object)), targetExp);
            dlgResult = (delgGetProperty)lambdaExp.Compile();
            return dlgResult;
        }
    }
}
