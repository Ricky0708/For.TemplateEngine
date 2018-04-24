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

        /// <summary>
        /// 由範本中抽取特殊標記的pattern
        /// </summary>
        private static Regex regex = new Regex("({.\\w*})");

        /// <summary>
        /// 建立範本的委派及cache
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        internal static Queue<NodeModel> BuildTemplate(Type type, string template)
        {

            if (!Caches.IsExist(CacheType.Template, template))
            {
                Caches.Lock();
                if (!Caches.IsExist(CacheType.Template, template))
                {
                    Caches.Add(CacheType.Template, template, _BuildTemplate(type, template));
                }
                Caches.Unlock();
            }

            var result = Caches.GetValue(CacheType.Template, template) as Queue<NodeModel>;
            if (result is null)
            {
                result = BuildTemplate(type, template);

            }
            return result;
        }

        internal static void ClearCache()
        {
            var enumAry = new CacheType[] {
                CacheType.Template,
            };

            Caches.Lock();
            foreach (var item in enumAry)
            {
                Caches.RemoveCache(item);
            }
            Caches.Unlock();
        }

        private static Queue<NodeModel> _BuildTemplate(Type type, string template)
        {
            var array = regex.Split(template);
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
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            MemberExpression propertyExp = Expression.Property(Expression.Convert(targetExp, type), prop);

            LambdaExpression lambdax = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(propertyExp, typeof(object)), targetExp);
            delgGetProperty delg = (delgGetProperty)lambdax.Compile();
            return delg;
        }
    }
}
