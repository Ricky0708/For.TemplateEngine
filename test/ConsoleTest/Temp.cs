using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class Temp
    {

    }

namespace Fz.Report.Common.Services.Implements.Localizations
    {
        /// <summary>
        /// 多語系剖析服務
        /// </summary>
        public class LocalizaionStringProcessor
        {
            /// <summary>
            /// 字典表
            /// </summary>
            public Dictionary<string, Dictionary<string, string>> Languages => _langCache;
            private delegate string delgGetData(object instance);
            private readonly Dictionary<string, Dictionary<string, string>> _langCache = new Dictionary<string, Dictionary<string, string>>();
            private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _processedCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
            private readonly ConcurrentDictionary<string, delgGetData> _delgCacheGetParam = new ConcurrentDictionary<string, delgGetData>();
            private readonly ConcurrentDictionary<string, delgGetData> _delgCacheGetProperty = new ConcurrentDictionary<string, delgGetData>();
            private readonly static Regex _reg = new Regex("(##)[^###](?!.*\\1).+?@@");

            /// <summary>
            /// 设定语系档
            /// </summary>
            /// <param name="dic"></param>
            /// <param name="langCode"></param>
            public void SetLanguage(Dictionary<string, string> dic, string langCode)
            {

                if (!_langCache.ContainsKey(langCode))
                {
                    lock (_langCache)
                    {
                        if (!_langCache.ContainsKey(langCode))
                        {
                            _langCache.Add(langCode, dic);
                        }
                    }
                }
                if (!_processedCache.ContainsKey(langCode))
                {
                    lock (_processedCache)
                    {
                        if (!_processedCache.ContainsKey(langCode))
                        {
                            _processedCache.TryAdd(langCode, new ConcurrentDictionary<string, string>());
                        }
                    }
                }

                _langCache[langCode] = dic;
                lock (_processedCache[langCode])
                {
                    _processedCache[langCode].Clear();
                }
                lock (_delgCacheGetParam)
                {
                    _delgCacheGetParam.Clear();
                }
                lock (_delgCacheGetProperty)
                {
                    _delgCacheGetProperty.Clear();
                }
            }

            /// <summary>
            /// 加入动态参数
            /// </summary>
            /// <param name="str"></param>
            /// <param name="paramData"></param>
            /// <returns></returns>
            public string LangAddParams(string str, object paramData)
            {
                var cacheKey = $"{str}{paramData.GetType().ToString()}";
                if (!_delgCacheGetProperty.TryGetValue(cacheKey, out var lambda))
                {
                    lock (_delgCacheGetProperty)
                    {
                        if (!_delgCacheGetProperty.TryGetValue(cacheKey, out lambda))
                        {
                            var exprList = new List<Expression>();
                            var targetExpr = Expression.Parameter(typeof(object), "target");
                            var memberExpr = Expression.Convert(targetExpr, paramData.GetType());
                            var props = paramData.GetType().GetProperties();
                            exprList.Add(Expression.Constant(str));
                            exprList.Add(Expression.Constant("^"));
                            foreach (var prop in props)
                            {
                                var propExpression = Expression.Property(memberExpr, prop);
                                var toStringExpression = default(MethodCallExpression);
                                if (propExpression.Type != typeof(string))
                                {
                                    var toString = propExpression.Type.GetMethods().First(p => p.Name.ToLower() == "tostring");
                                    toStringExpression = Expression.Call(propExpression, toString);
                                }
                                var constExpression = Expression.Constant(propExpression.Member.Name);
                                exprList.Add(constExpression);
                                exprList.Add(Expression.Constant("^"));
                                exprList.Add(toStringExpression != null ? toStringExpression as Expression : propExpression);
                                exprList.Add(Expression.Constant("^"));
                            }
                            exprList.RemoveAt(exprList.Count - 1);
                            var method = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });
                            var paramsExpr = Expression.NewArrayInit(typeof(object), exprList);
                            var methodExpr = Expression.Call(method, paramsExpr);
                            var lambdaExpr = Expression.Lambda<delgGetData>(methodExpr, targetExpr);
                            lambda = lambdaExpr.Compile();
                            _delgCacheGetProperty.TryAdd(cacheKey, lambda);
                        }
                    }
                }

                return $"##{lambda.Invoke(paramData)}@@";

                //var param = JsonConvert.SerializeObject(new { Key = str.Replace("{", "").Replace("}", ""), Data = paramData });
                //return $"##{param}";
            }

            /// <summary>
            /// 转换为多语系
            /// </summary>
            /// <param name="str"></param>
            /// <param name="lang"></param>
            /// <returns></returns>
            public string Localize(string str, string lang)
            {
                if (string.IsNullOrEmpty(str)) return str;
                var paramModel = default(Dictionary<string, string>);
                var match = default(Match);
                var isSharpSharp = false;
                while ((match = _reg.Match(str)).Length != 0)
                {
                    isSharpSharp = true;
                    var obj = match.Value.Substring(2, match.Value.Length - 4).Split('^');
                    var tempResult = obj[0];
                    paramModel = new Dictionary<string, string>();
                    for (int i = 1; i < obj.Length - 1; i += 2)
                    {
                        paramModel.Add(obj[i], obj[i + 1]);
                    }
                    str = _reg.Replace(str, Parser(tempResult, lang, paramModel));
                }
                if (isSharpSharp) return str;
                //if (str.StartsWith("##"))
                //{
                //    var obj = str.Substring(2).Split('^');
                //    str = obj[0];
                //    paramModel = new Dictionary<string, string>();
                //    for (int i = 1; i < obj.Length - 1; i += 2)
                //    {
                //        paramModel.Add(obj[i], obj[i + 1]);
                //    }
                //    return Parser(str, lang, paramModel);
                //}

                if (!_processedCache[lang].TryGetValue(str, out var result))
                {
                    lock (_processedCache[lang])
                    {
                        if (!_processedCache[lang].TryGetValue(str, out result))
                        {
                            result = Parser(str, lang, paramModel);
                            _processedCache[lang].TryAdd(str, result);
                        }
                    }
                }

                return result;
            }

            private string Parser(string str, string lang, object paramModel)
            {
                var sb = new StringBuilder();
                var start = false;
                var key = new StringBuilder();
                var isParam = false;
                var result = "";
                foreach (var chr in str)
                {
                    if (chr == '{')
                    {
                        start = true;
                    }
                    else if (chr == '}')
                    {
                        if (isParam)
                        {
                            sb.Append(Parser(ProcessParam(key.ToString(), paramModel).Invoke(paramModel), lang, paramModel));
                        }
                        else
                        {
                            sb.Append(Parser(_langCache[lang][key.ToString()], lang, paramModel));
                        }
                        key.Clear();
                        start = false;
                        isParam = false;
                    }
                    else if (start && chr == '#')
                    {
                        isParam = true;
                    }
                    else
                    {
                        if (start)
                        {
                            key.Append(chr);
                        }
                        else
                        {
                            sb.Append(chr);
                        }
                    }

                }
                result = sb.ToString();
                return result;
            }

            private delgGetData ProcessParam(string str, object paramModel)
            {
                if (!_delgCacheGetParam.TryGetValue(str, out var lambda))
                {
                    lock (_delgCacheGetParam)
                    {
                        if (!_delgCacheGetParam.TryGetValue(str, out lambda))
                        {
                            var sb = new StringBuilder();
                            var key = new StringBuilder();
                            //var exprList = new List<Expression>();
                            var targetExpr = Expression.Parameter(typeof(object), "target");
                            var memberExpr = Expression.Convert(targetExpr, paramModel.GetType());

                            var keyExpr = Expression.Constant(str);
                            PropertyInfo indexer = (from p in memberExpr.Type.GetDefaultMembers().OfType<PropertyInfo>()
                                                    where p.PropertyType == typeof(string)
                                                    let q = p.GetIndexParameters()
                                                    where q.Length == 1 && q[0].ParameterType == typeof(string)
                                                    select p).Single();
                            IndexExpression indexExpr = Expression.Property(memberExpr, indexer, keyExpr);
                            //exprList.Add(indexExpr);

                            key.Clear();
                            sb.Clear();

                            //var method = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });
                            //var paramsExpr = Expression.NewArrayInit(typeof(object), exprList);
                            //var methodExpr = Expression.Call(method, paramsExpr);
                            var lambdaExpr = Expression.Lambda<delgGetData>(indexExpr, targetExpr);
                            lambda = lambdaExpr.Compile();
                            _delgCacheGetParam.TryAdd(str, lambda);
                        }
                    }
                }

                return lambda;
            }
        }
    }

}
