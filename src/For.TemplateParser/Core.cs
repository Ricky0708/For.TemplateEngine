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
        private static Regex _regexProperty => new Regex(@"({\..*?})");
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

        internal delgGetProperty BuildTemplate2(Type type, string template)
        {

            if (!_templateCache.IsExist(type.ToString()))
            {
                _templateCache.Lock();
                if (!_templateCache.IsExist(type.ToString()))
                {
                    _templateCache.Add(type.ToString(), _BuildTemplate2(type, template));
                }
                _templateCache.Unlock();
            }

            var result = _templateCache.GetValue(type.ToString()) as delgGetProperty;
            if (result is null)
            {
                result = BuildTemplate2(type, template);

            }
            return result;
        }

        internal void ClearCache()
        {
            _templateCache.Lock();
            _templateCache.RemoveCache();
            _templateCache.Unlock();
        }

        [Obsolete("舊的方式，不再使用")]
        private static Queue<NodeModel> _BuildTemplate(Type type, string template)
        {
            var forPropertyArray = _regexProperty.Split(template);
            var forListArray = _regexList.Split(template);
            var que = new Queue<NodeModel>();
            foreach (var item in forPropertyArray)
            {
                if (item.StartsWith("{."))
                {
                    que.Enqueue(new NodeModel()
                    {
                        Type = NodeType.Property,
                        NodeDelegateValue = _BuildGetPropertyMethod(type, item.Replace("{.", "").Replace("}", "").Split('.')),
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

        private static delgGetProperty _BuildTemplate2(Type type, string template)
        {
            var forPropertyArray = _regexProperty.Split(template);
            MethodInfo method = typeof(string).GetMethod("Concat", new Type[] { typeof(object[]) });
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            var memberExp = Expression.Convert(targetExp, type);
            List<Expression> exprList = new List<Expression>();
            foreach (var item in forPropertyArray)
            {
                if (item.StartsWith("{."))
                {
                    exprList.Add(_BuildGetPropertyExpr(memberExp, item.Replace("{.", "").Replace("}", "").Split('.')));
                }
                else
                {
                    exprList.Add(_BuildConstExpr(item));
                }
            }
            var parametersExpression = Expression.NewArrayInit(typeof(object), exprList);
            MethodCallExpression methodExp = Expression.Call(method, parametersExpression);
            var l = Expression.Lambda<delgGetProperty>(methodExp, targetExp);
            var ll = l.Compile() as delgGetProperty;
            //ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            //MethodInfo method = typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) });
            //Expression memberExp = Expression.Convert(targetExp, typeof(TModel));
            //Expression memberExp2 = Expression.Property(memberExp, "Name");
            //Expression constExpr = Expression.Constant("AA");
            //var temp = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(memberExp2, typeof(object)), targetExp);
            //var lambdaExp = temp.Compile();
            //MethodCallExpression methodExp = Expression.Call(method, constExpr, memberExp2);
            //var l = Expression.Lambda(methodExp, targetExp);
            //var ll = l.Compile();
            //var s = ll.DynamicInvoke(new TModel(){ Name = "QQ" }); 
            return ll;
        }
        private class TModel
        {
            public string Name { get; set; }
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
        private static Expression _BuildGetPropertyExpr(Expression targetExp, params string[] props)
        {
            Expression memberExp = targetExp;

            for (int i = 0; i < props.Length; i++)
            {
                memberExp = Expression.Property(memberExp, props[i]);
            }

            return Expression.Convert(memberExp, typeof(object));
        }
        private static Expression _BuildConstExpr(string value)
        {
            return Expression.Constant(value);
        }

    }
}
