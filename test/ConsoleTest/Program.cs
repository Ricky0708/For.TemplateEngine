using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using For.TemplateEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestA();
            //TestB();
            //TestC();
            //TestDForDemo();
            TestDForDemoB();
        }

        static void TestDForDemoB()
        {
            #region Init dic


            var sentenceKey = "sentenceKey";

            var dicZh = new Dictionary<string, string>();
            var dicEn = new Dictionary<string, string>();
            dicZh.Add(sentenceKey, "嗨! 這是{#Game}，我是玩家{#Player}，這是{System}");
            dicZh.Add("PK10", "英國賽車");
            dicZh.Add("MarkSix", "六合");
            dicZh.Add("System", "方舟六");

            dicEn.Add(sentenceKey, "Hi, I'm {#Player}, this is {System}, the game is {#Game}");
            dicEn.Add("PK10", "PK10");
            dicEn.Add("MarkSix", "MarkSix");
            dicEn.Add("System", "FZ6");

            Extension.SetLanguages(dicZh, "zh");
            Extension.SetLanguages(dicEn, "en");

            #endregion

            var a = "";
            var b = "";
            var json = JsonConvert.SerializeObject(new { Player = "Ricky", Game = "{MarkSix}" });
            var jb = JsonConvert.DeserializeObject(json);

            Action parallerRenderA = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    a = Extension.GetMessage(sentenceKey, "zh", jb);
                    //a = Extension.GetMessage(sentenceKey, "zh", new { ProfileAge = "60", AA = "AAA", BB = "BBB", MyC = "CCC" });
                });
            };
            Action parallerRenderB = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    a = Extension.GetMessage(sentenceKey, "en", jb);
                });
            };

            Watch("parallerRenderA 有動態參數", parallerRenderA);
            Watch("parallerRenderB 無動態參數", parallerRenderB);

            json = JsonConvert.SerializeObject(new { Player = "Ricky77777", Game = "{System}" });
            jb = JsonConvert.DeserializeObject(json);

            a = Extension.GetMessage(sentenceKey, "zh", jb);
            b = Extension.GetMessage(sentenceKey, "en", jb);
            Console.Write("\r\n\r\n");
            Console.Write(a);
            Console.Write("\r\n\r\n");
            Console.Write(b);
            Console.ReadLine();
        }

        static void Watch(string tag, Action p)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            p.Invoke();
            watch.Stop();
            Console.WriteLine($"[{tag}] {watch.ElapsedMilliseconds}");
            watch.Reset();
        }
    }

    public static class Extension
    {
        private delegate string delgGetProperty(object instance);

        private static Dictionary<string, Dictionary<string, string>> _cache = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, delgGetProperty> _delgCache = new Dictionary<string, delgGetProperty>();

        public static void SetLanguages(Dictionary<string, string> dic, string langCode) => _cache.Add(langCode, dic);

        public static string AddParams(this string str, object paramData)
        {
            // LangTABLE 
            // IndexId, Template, ParamData

            var param = JsonConvert.SerializeObject(paramData);
            var sbSQL = new StringBuilder();
            sbSQL.Append($"INSERT INTO LangTable (Template, ParamData) ");
            sbSQL.Append($"VALUES ('{str}', '{param}') ");
            sbSQL.Append($"return IndexId");
            var indexId = 1;

            return $"##{indexId.ToString()}";
        }

        public static string GetMessage(string code, string langCode, object paramModel = null)
        {
            if (_cache.TryGetValue(langCode, out var langDic))
            {
                if (langDic.TryGetValue(code, out var result))
                {

                    return result.ConvertToResult(langCode, paramModel);
                }
            }
            return "";
        }

        public static string ConvertToResult(this string str, string lang, object paramModel)
        {
            var resultString = ProcessParam(str, paramModel).Invoke(paramModel);
            return ProcessString(resultString, lang);
        }

        private static string ProcessString(this string str, string lang)
        {
            var sb = new StringBuilder();
            var start = false;
            var key = new StringBuilder();
            if (!_cache[lang].TryGetValue(str, out var result))
            {
                lock (_cache[lang])
                {
                    if (!_cache[lang].TryGetValue(str, out result))
                    {
                        foreach (var chr in str)
                        {
                            if (chr == '{')
                            {
                                start = true;
                            }
                            else if (chr == '}')
                            {

                                sb.Append(_cache[lang][key.ToString()].ProcessString(lang));
                                key.Clear();
                                start = false;
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
                        _cache[lang].Add(str, result);
                    }
                }
            }

            return result;
        }

        private static delgGetProperty ProcessParam(string str, object paramModel)
        {
            if (!_delgCache.TryGetValue(str, out var lambda))
            {
                lock (_delgCache)
                {
                    if (!_delgCache.TryGetValue(str, out lambda))
                    {
                        var sb = new StringBuilder();
                        var start = false;
                        var key = new StringBuilder();
                        var exprList = new List<Expression>();
                        var targetExpr = Expression.Parameter(typeof(object), "target");
                        var memberExpr = Expression.Convert(targetExpr, paramModel.GetType());
                        var isParam = false;

                        foreach (var chr in str)
                        {
                            if (chr == '{')
                            {
                                if (sb.Length != 0) exprList.Add(Expression.Constant(sb.ToString()));
                                sb.Clear();
                                start = true;
                            }
                            else if (chr == '}')
                            {

                                //var o = JObject.Parse("{\"ProfileAge\":\"60\",\"AA\":\"AAA\",\"BB\":\"BBB\",\"MyC\":\"CCC\"}");
                                //o.SelectTokens("AA").First().Value<string>();
                                if (isParam)
                                {
                                    var jMethodSelectToken = typeof(JToken).GetMethod("SelectTokens", new[] { typeof(string) });
                                    var jMethodFirst = typeof(Enumerable).GetMethods().Where(p => p.Name == "First" && p.GetParameters().Count() == 1).First();
                                    var jMethodValue = typeof(JToken).GetMethods().Where(m =>
                                            m.Name == "Value"
                                            ).First();
                                    var mx = Expression.Call(memberExpr, jMethodSelectToken, Expression.Constant(key.ToString()));
                                    var mx2 = Expression.Call(typeof(Enumerable), "First", new Type[] { typeof(JToken) }, mx);
                                    var mx3 = Expression.Call(typeof(Newtonsoft.Json.Linq.Extensions), "Value", new Type[] { typeof(string) }, mx2);

                                    exprList.Add(mx3);
                                }
                                else
                                {
                                    exprList.Add(Expression.Constant($"{{{key}}}"));
                                }

                                //exprList.Add(Expression.Property(memberExpr, key.ToString()));
                                key.Clear();
                                sb.Clear();
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
                        if (sb.Length != 0) exprList.Add(Expression.Constant(sb.ToString()));

                        var method = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });
                        var paramsExpr = Expression.NewArrayInit(typeof(object), exprList);
                        var methodExpr = Expression.Call(method, paramsExpr);
                        var lambdaExpr = Expression.Lambda<delgGetProperty>(methodExpr, targetExpr);
                        lambda = lambdaExpr.Compile();
                        _delgCache.Add(str, lambda);
                    }
                }
            }

            return lambda;
        }

        private static Expression GenerateGetterLambda(PropertyInfo property)
        {
            // Define our instance parameter, which will be the input of the Func
            var objParameterExpr = Expression.Parameter(typeof(object), "instance");
            // 1. Cast the instance to the correct type
            var instanceExpr = Expression.TypeAs(objParameterExpr, property.DeclaringType);
            // 2. Call the getter and retrieve the value of the property
            var propertyExpr = Expression.Property(instanceExpr, property);
            // 3. Convert the property's value to object
            var propertyObjExpr = Expression.Convert(propertyExpr, typeof(object));
            // Create a lambda expression of the latest call & compile it
            return Expression.Lambda<Func<object, object>>(propertyObjExpr, objParameterExpr);
        }
    }
}
