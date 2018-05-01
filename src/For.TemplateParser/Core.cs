using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using For.TemplateParser.Caches;
using For.TemplateParser.Models;

namespace For.TemplateParser
{
    internal class Core
    {
        internal delegate object delgGetProperty(object instance);
        private readonly ITemplateCacheProvider _templateCache;

        /// <summary>
        /// 由範本中抽取特殊標記的pattern
        /// </summary>
        private static Regex _regexProperty => new Regex(@"({\..*?})");
        private readonly TemplateParserConfig _templateParserConfig;

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

        /// <summary>
        /// register template and cache
        /// </summary>
        /// <param name="type"></param>
        /// <param name="template"></param>
        /// <param name="cacheKey"></param>
        internal void RegisterTemplate(Type type, string template, string cacheKey)
        {
            var count = 0;
            while (true)
            {
                count += 1;
                _templateCache.Lock();
                if (!_templateCache.IsExist(cacheKey))
                {
                    _templateCache.Add(cacheKey, _BuildTemplateInDelegate(type, template));
                }
                else
                {
                    _templateCache.Reset(cacheKey, _BuildTemplateInDelegate(type, template));
                }
                _templateCache.Unlock();

                if (GetTemplateDelegate(cacheKey) is null)
                {
                    continue;
                }
                System.Threading.Thread.Sleep(500);
                if (count > 5)
                {
                    throw new Exception("Can't generate template");
                }
                break;
            }
        }

        /// <summary>
        /// get delegate method by cache key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        internal delgGetProperty GetTemplateDelegate(string cacheKey)
        {
            var result = _templateCache.GetValue(cacheKey) as delgGetProperty;
            return result;
        }

        /// <summary>
        /// use type and template to build delegate
        /// </summary>
        /// <param name="type"></param>
        /// <param name="template"></param>
        /// <returns></returns>
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

        /// <summary>
        /// generate member expression
        /// </summary>
        /// <param name="targetExp"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        private Expression _BuildGetPropertyExpr(Expression targetExp, params string[] props)
        {
            var memberExp = props.Aggregate(targetExp, Expression.Property);
            return _BuildToStringExpression(memberExp);
        }

        /// <summary>
        /// generate  "ToString(object[])" MethodCallExpression
        /// </summary>
        /// <param name="memberExp"></param>
        /// <returns></returns>
        private Expression _BuildToStringExpression(Expression memberExp)
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

        /// <summary>
        /// generate const expression
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Expression _BuildConstExpr(string value)
        {
            return Expression.Constant(value);
        }

        internal void ClearCache()
        {
            _templateCache.Lock();
            _templateCache.RemoveCache();
            _templateCache.Unlock();
        }
    }
}
