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
            var sentenceKey2 = "sentenceKey2";

            var dicZh = new Dictionary<string, string>();
            var dicEn = new Dictionary<string, string>();
            dicZh.Add(sentenceKey, "嗨! 這是{#Game}，我是玩家{#Player}，這是{System}");
            dicZh.Add(sentenceKey2, "嗨! 這是{PK10}，我是玩家{MarkSix}，這是{System}");
            dicZh.Add("PK10", "英國賽車");
            dicZh.Add("MarkSix", "六合");
            dicZh.Add("System", "方舟六");

            dicEn.Add(sentenceKey, "Hi, I'm {#Player}, this is {System}, the game is {#Game}");
            dicEn.Add(sentenceKey2, "Hi, I'm {MarkSix}, this is {System}, the game is {PK10}");
            dicEn.Add("PK10", "PK10");
            dicEn.Add("MarkSix", "MarkSix");
            dicEn.Add("System", "FZ6");

            Extension.SetLanguages(dicZh, "zh");
            Extension.SetLanguages(dicEn, "en");

            #endregion

            var nn = JsonConvert.SerializeObject(new
            {
                Key = "sentenceKey",
                Data = new { Player = "Ricky", Game = "{MarkSix}" }
            });

            var a = "";
            var b = "";
            var json = JsonConvert.SerializeObject(new { Player = "Ricky", Game = "{MarkSix}" });
            var jb = JsonConvert.DeserializeObject(json);
            var x = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{PK10}" });
            Action parallerRenderA = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    a = x.Localize("zh");
                    //a = Extension.GetMessage(sentenceKey, "zh", new { ProfileAge = "60", AA = "AAA", BB = "BBB", MyC = "CCC" });
                });
            };
            Action parallerRenderB = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    b = $"{sentenceKey2}".Localize("zh");
                });
            };

            Watch("句子 動態取代  ", parallerRenderA);
            Watch("句子 無動態取代", parallerRenderB);

            //json = JsonConvert.SerializeObject(new { Player = "Ricky77777", Game = "{System}" });
            //jb = JsonConvert.DeserializeObject(json);

            x = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{MarkSix}" });
            Console.WriteLine(x.Localize("zh"));
            Console.Write("\r\n\r\n");
            x = "{sentenceKey}".AddParams(new { Player = "Ricky777", Game = "{PK10}" });
            Console.WriteLine(x.Localize("en"));

            // 寫入 DB
            // DB中有一張表作為對應，有三個欄位，Id, KeyCode, ParamData(json)
            // 當呼叫AddParams的時候，會將model轉成json，並將KeyCode及ParamData寫入這張表，並將自增值id反回
            // 實際logRemark的欄位加上特殊標記##後存到logRemark中，如這個案例會在 logRemark放進 ##1
            // 並在對應表中放進  1, sentenceKey, {"Player":"Ricky","Game":"{MarkSix}"}"

            // --zh:"嗨! 這是{#Game}，我是玩家{#Player}，這是{System}--"
            // --en:"Hi, I'm {#Player}, this is {System}, the game is {#Game}--"
            var logRemark = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{PK10}" });

            // 從 DB 讀出
            // 如果語句開頭標示為 ##，取出##後面的id，到對應表中拿到 key跟參數，進行處理
            var displayLogZh = logRemark.Localize("zh");
            var displayLogEn = logRemark.Localize("en");

            // 一般對應
            // 不使用
            var word = "{System}-{MarkSix}#{PK10}";
            var displayWordZh = word.Localize("zh");
            var displayWordEn = word.Localize("en");

            Console.Write("\r\n\r\n");
            Console.Write($"寫入 DB的值     : {logRemark}");
            Console.Write("\r\n\r\n");
            Console.Write($"從db讀出後轉換 Zh: {displayLogZh}");
            Console.Write("\r\n\r\n");
            Console.Write($"從db讀出後轉換 En: {displayLogEn}");

            Console.Write("\r\n\r\n");
            Console.Write("--------------------------------");
            Console.Write("\r\n\r\n");
            Console.Write($"寫入 DB的值     : {word}");
            Console.Write("\r\n\r\n");
            Console.Write($"直接轉換zh: {displayWordZh}");
            Console.Write("\r\n\r\n");
            Console.Write($"直接轉換en: {displayWordEn}");
            Console.Write("\r\n\r\n");
            Console.Write("--------------------------------");
            Console.Write("\r\n\r\n");
            Console.Write($"正常語句不受影響可相容既有資料: {"這是皆凱科技".Localize("zh")}");

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

    public class KeyModel
    {
        public string Key { get; set; }
        public JObject Data { get; set; }
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
            // 這裡要實做寫入 db
            var param = JsonConvert.SerializeObject(new { Key = str.Replace("{", "").Replace("}", ""), Data = paramData });
            return $"##{param}";
        }


        public static string Localize(this string str, string lang)
        {
            var paramModel = default(object);
            var resultString = "";
            if (str.StartsWith("##"))
            {
                var obj = JsonConvert.DeserializeObject<KeyModel>(str.Substring(2));
                str = obj.Key;
                paramModel = obj.Data;
            }
            if (_cache.TryGetValue(lang, out var langDic))
            {
                if (langDic.TryGetValue(str, out var result))
                {
                    resultString = paramModel == null ? str : ProcessParam(result, paramModel).Invoke(paramModel);
                    return ProcessString(resultString, lang);
                }
            }
            resultString = paramModel == null ? str : ProcessParam(str, paramModel).Invoke(paramModel);
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
