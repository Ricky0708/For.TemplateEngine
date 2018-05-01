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

        private TemplateParserConfig _templateParserConfig;
        //private static Regex _regexList => new Regex(@"(\[\#.*?(\#])+)");

        internal Core(TemplateParserConfig templateParserConfig)
        {
            _templateCache = new DefaultTemplateCacheProvider();
            _templateParserConfig = templateParserConfig;
        }
        internal Core(ITemplateCacheProvider templateCache, TemplateParserConfig templateParserConfig)
        {
            _templateCache = templateCache;
            _templateParserConfig = templateParserConfig;
        }

        internal delgGetProperty BuildTemplateInDelegate<T>(string template, string cacheKey)
        {
            var type = typeof(T);
            if (!_templateCache.IsExist(cacheKey))
            {
                _templateCache.Lock();
                if (!_templateCache.IsExist(cacheKey))
                {
                    _templateCache.Add(cacheKey, _BuildTemplateInDelegate(type, template));
                }
                _templateCache.Unlock();
            }

            var result = _templateCache.GetValue(cacheKey) as delgGetProperty;
            if (result is null)
            {
                result = BuildTemplateInDelegate<T>(template, cacheKey);

            }
            return result;
        }

        public void RegisterTemplate<T>(string template, string cacheKey)
        {
            var type = typeof(T);
            if (!_templateCache.IsExist(cacheKey))
            {
                _templateCache.Lock();
                if (!_templateCache.IsExist(cacheKey))
                {
                    _templateCache.Add(cacheKey, _BuildTemplateInDelegate(type, template));
                }
                _templateCache.Unlock();
            }

            if (GetTemplateDelegate(cacheKey) is null)
            {
                BuildTemplateInDelegate<T>(template, cacheKey);
            }
        }

        internal delgGetProperty GetTemplateDelegate(string cacheKey)
        {
            var result = _templateCache.GetValue(cacheKey) as delgGetProperty;
            return result;
        }

        internal delgGetProperty ReBuildTemplateInDelegate<T>(string template, string cacheKey)
        {
            var type = typeof(T);
            if (_templateCache.IsExist(cacheKey))
            {
                _templateCache.Lock();
                if (_templateCache.IsExist(cacheKey))
                {
                    _templateCache.Reset(cacheKey, _BuildTemplateInDelegate(type, template));
                }
                else
                {
                    BuildTemplateInDelegate<T>(template, cacheKey);
                }
                _templateCache.Unlock();
            }
            else
            {
                BuildTemplateInDelegate<T>(template, cacheKey);
            }

            var result = _templateCache.GetValue(cacheKey) as delgGetProperty;
            if (result is null)
            {
                result = BuildTemplateInDelegate<T>(template, cacheKey);
            }
            return result;
        }


        internal void ClearCache()
        {
            _templateCache.Lock();
            _templateCache.RemoveCache();
            _templateCache.Unlock();
        }

        private delgGetProperty _BuildTemplateInDelegate(Type type, string template)
        {
            var forPropertyArray = _regexProperty.Split(template);
            var method = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });
            var targetExp = Expression.Parameter(typeof(object), "target");
            var memberExp = Expression.Convert(targetExp, type);
            var exprList = forPropertyArray.Select(item => item.StartsWith("{.")
                    ? _BuildGetPropertyExpr(memberExp, item.Replace("{.", "").Replace("}", "").Split('.'))
                    : _BuildConstExpr(item))
                .ToList();

            var parametersExpression = Expression.NewArrayInit(typeof(object), exprList);

            var methodExp = Expression.Call(method, parametersExpression);
            var lambdaExpr = Expression.Lambda<delgGetProperty>(methodExp, targetExp);
            var lambda = lambdaExpr.Compile();
            return lambda;
        }
        private Expression _BuildGetPropertyExpr(Expression targetExp, params string[] props)
        {
            var memberExp = props.Aggregate(targetExp, Expression.Property);
            return _GetToStringExpression(memberExp);
        }
        private Expression _GetToStringExpression(Expression memberExp)
        {
            var propType = ((PropertyInfo)(memberExp as MemberExpression).Member).PropertyType;
            MethodInfo method;
            switch (propType.Name.ToLower())
            {
                case "datetimeoffset":
                    if (!string.IsNullOrEmpty(_templateParserConfig.DateTimeOffsetFormat))
                    {
                        method = propType.GetMethod("ToString", new[] { typeof(string) });
                        return Expression.Call(memberExp, method, _BuildConstExpr(_templateParserConfig.DateTimeOffsetFormat));
                    }
                    break;
                case "datetime":
                    if (!string.IsNullOrEmpty(_templateParserConfig.DateTimeOffsetFormat))
                    {
                        method = propType.GetMethod("ToString", new[] { typeof(string) });
                        return Expression.Call(memberExp, method, _BuildConstExpr(_templateParserConfig.DateTimeFormat));
                    }
                    break;
            }
            return Expression.Convert(memberExp, typeof(object));
        }
        private static Expression _BuildConstExpr(string value)
        {
            return Expression.Constant(value);
        }

        /// <summary>
        /// 建立範本的委派及cache
        /// </summary>
        /// <param name="type"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        [Obsolete("舊的方式，不再使用")]
        internal Queue<NodeModel> BuildTemplateInQue(Type type, string template)
        {

            if (!_templateCache.IsExist(template))
            {
                _templateCache.Lock();
                if (!_templateCache.IsExist(template))
                {
                    _templateCache.Add(template, _BuildTemplateInQue(type, template));
                }
                _templateCache.Unlock();
            }

            var result = _templateCache.GetValue(template) as Queue<NodeModel>;
            if (result is null)
            {
                result = BuildTemplateInQue(type, template);

            }
            return result;
        }
        [Obsolete("舊的方式，不再使用")]
        private static Queue<NodeModel> _BuildTemplateInQue(Type type, string template)
        {
            var forPropertyArray = _regexProperty.Split(template);
            //var forListArray = _regexList.Split(template);
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
        [Obsolete("舊的方式，不再使用")]
        private static delgGetProperty _BuildGetPropertyMethod(Type type, params string[] props)
        {
            delgGetProperty dlgResult = null;
            var targetExp = Expression.Parameter(typeof(object), "target");
            Expression memberExp = Expression.Convert(targetExp, type);

            memberExp = props.Aggregate(memberExp, Expression.Property);
            var lambdaExp = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(memberExp, typeof(object)), targetExp);
            dlgResult = (delgGetProperty)lambdaExp.Compile();
            return dlgResult;
        }

   
    }
}
