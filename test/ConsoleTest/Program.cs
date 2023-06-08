using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleTest
{
    public class KeyModel
    {
        public string Key { get; set; }
        public JObject Data { get; set; }
    }

    public class QQ
    {
        public string Player { get; set; }
        public string Game { get; set; }
        public string System { get; set; }
    }

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

            var dicZh = new Dictionary<string, string>() {
                { "BetNo2121", "二全中" },
                { "BetNo2123", "二特串" },
                { "BetNo2141", "四全中" },
                { "BetNo6103", "过关" },
                { "BetNo112001", "正1大" },
                { "BetNo122001", "正2大" },
                { "BetNo132001", "正3大" },
                { "BetNo142001", "正4大" },
                { "BetNo152001", "正5大" },
                { "BetNo2231", "三肖连中" },
                { "BetNo2501", "五中一" },
                { "BetNo2251", "五肖连中" },

                { "Zodiac1", "鼠" },
                { "Zodiac2", "牛" },
                { "Zodiac3", "虎" },
                { "Zodiac4", "兔" },
                { "Zodiac5", "龙" },
                { "Zodiac6", "蛇" },
                { "Zodiac7", "马" },
                { "Zodiac8", "羊" },

                { "Tail1", "1尾" },
                { "Tail2", "2尾" },
                { "Tail3", "3尾" },
                { "Tail4", "4尾" },

                { "Drag", "拖" },
                { "Bump", "碰" }
            };
            var dicEn = new Dictionary<string, string>();
            dicZh.Add(sentenceKey, "嗨! 這是{#Game}，我是玩家{#Player}，這是{System}");
            dicZh.Add(sentenceKey2, "嗨! 這是{PK10}，我是玩家{MarkSix}，這是{System}");
            dicZh.Add("PK10", "英國賽車");
            dicZh.Add("MarkSix", "六合");
            dicZh.Add("System", "方舟六");
            dicZh.Add("testWork", "修改{#Agent}设置 - 调整信用余额[{#Game}](分给下级{#Player} 额度{#Amount})");

            for (int i = 0; i < 100000; i++)
            {
                dicZh.Add($"BetNo.Key{i}", $"BetNoKey{i}");
            }

            dicEn.Add(sentenceKey, "Hi, I'm {#Player}, this is {System}, the game is {#Game}");
            dicEn.Add(sentenceKey2, "Hi, I'm {MarkSix}, this is {System}, the game is {PK10}");
            dicEn.Add("PK10", "PK10");
            dicEn.Add("MarkSix", "MarkSix");
            dicEn.Add("System", "FZ6");
            dicEn.Add("testWork", "修改{Agent}设置 - 调整信用余额[{Game}](分给下级{Player} 额度{Amount})");

            LocalizationUtil.AddLanguage(dicZh, "zh");
            LocalizationUtil.AddLanguage(dicEn, "en");

            //var logRemark = "{testWork}".AddParams(new {
            //        Agent ="AAAAA",
            //        Game = "$PK10",
            //        Player = "Rucky", 
            //        Amount = "100",
            //        Remark= "修改{Agent}设置 - 调整信用余额[{Game}](分给下级{Player} 额度{Amount})"
            //});
            #endregion

            var x = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{PK10}" });
            var a = "";
            var b = "";
            var c = "";
            var d = "";
            var e = "";
            //var txt = "";

            //Console.WriteLine("---");
            //Console.WriteLine(x.Localize("zh"));
            //Console.WriteLine("\r\n\r\n");
            //Console.WriteLine(q.Localize("zh"));
            var nn = new QQ
            {
                Game = "Game",
                Player = "Player",
                System = "System",
            };
            var xx = JsonConvert.SerializeObject(nn);
            Action parallerRenderA = () =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    a = "{BetNo2251}-{Zodiac1},{Zodiac2},{Zodiac3},{Zodiac4} [{Drag}] {Zodiac5},{Zodiac6},{Zodiac7},{Zodiac8}{BetNo6103}-{BetNo112001},{BetNo122001},{BetNo132001},{BetNo142001},{BetNo152001}"
                    .AddParams(new { GameA = "AA", GameB = "BB", GameC = "CC", GameD = "DD", GameE = "EE", GameF = "FF" });
                }
            };
            Action parallerRenderB = () =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    b = "{BetNo2251}-{Zodiac1},{Zodiac2},{Zodiac3},{Zodiac4} [{Drag}] {Zodiac5},{Zodiac6},{Zodiac7},{Zodiac8}{BetNo6103}-{BetNo112001},{BetNo122001},{BetNo132001},{BetNo142001},{BetNo152001}".Localize("zh");
                }
            };
            Action parallerRenderC = () =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    c = x.Localize("zh");
                }
            };

            Action parallerRenderD = () =>
            {
                for (int i = 0; i < 10000; i += 3)
                {
                    d = $"{{BetNo.Key{i}}},{{BetNo.Key{i + 1}}},{{BetNo.Key{i + 2}}}".Localize("zh");
                }
            };

            Action parallerRenderE = () =>
            {
                for (int i = 15000; i < 16000; i += 3)
                {
                    e = $"{{BetNo.Key{i}}},{{BetNo.Key{i + 1}}},{{BetNo.Key{i + 2}}}".Localize("zh");
                }
            };

   

            //Watch($"1.Add Params 6個 Property   ", parallerRenderA);
            //Watch($"2.句子 無動態取代 ", parallerRenderB);
            //Watch($"3.句子 動態取代 2個動態參數+1個靜態參數+1個由動態轉靜態的參數", parallerRenderC);
            Watch($"4.靜態句子 1萬次 ", parallerRenderD);
            Watch($"5.靜態句子 1千次 ", parallerRenderE);
            //Watch($"句子 Replace5 ", actionReplace5);
            Console.WriteLine("\r\n");
            Console.WriteLine("1:" + a);
            Console.WriteLine("2:" + b);
            Console.WriteLine("3:" + c);
            Console.WriteLine("4:" + d);
            Console.WriteLine("5:" + e);
            //Console.WriteLine(txt);


            //x = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{MarkSix}" });
            //Console.WriteLine(x.Localize("zh"));
            //Console.Write("\r\n\r\n");
            //x = "{sentenceKey}".AddParams(new { Player = "Ricky777", Game = "{PK10}" });
            //Console.WriteLine(x.Localize("zh"));


            //// --zh:"嗨! 這是{#Game}，我是玩家{#Player}，這是{System}--"
            //// --en:"Hi, I'm {#Player}, this is {System}, the game is {#Game}--"
            //var logRemark = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{PK10}" });

            //// 從 DB 讀出
            //// 如果語句開頭標示為 ##，取出##後面的id，到對應表中拿到 key跟參數，進行處理
            //var displayLogZh = logRemark.Localize("zh");
            //logRemark = "{sentenceKey}".AddParams(new { Player = "Ricky", Game = "{PK10}" });
            //var displayLogEn = logRemark.Localize("en");

            //// 一般對應
            //// 不使用
            //var word = "{System}-{MarkSix}#{PK10}";
            //var displayWordZh = word.Localize("zh");
            //var displayWordEn = word.Localize("en");

            //Console.Write("\r\n\r\n");
            //Console.Write($"寫入 DB的值     : {logRemark}");
            //Console.Write("\r\n\r\n");
            //Console.Write($"從db讀出後轉換 Zh: {displayLogZh}");
            //Console.Write("\r\n\r\n");
            //Console.Write($"從db讀出後轉換 En: {displayLogEn}");

            //Console.Write("\r\n\r\n");
            //Console.Write("--------------------------------");
            //Console.Write("\r\n\r\n");
            //Console.Write($"寫入 DB的值     : {word}");
            //Console.Write("\r\n\r\n");
            //Console.Write($"直接轉換zh: {displayWordZh}");
            //Console.Write("\r\n\r\n");
            //Console.Write($"直接轉換en: {displayWordEn}");
            //Console.Write("\r\n\r\n");
            //Console.Write("--------------------------------");
            //Console.Write("\r\n\r\n");
            //Console.Write($"正常語句不受影響可相容既有資料: {"這是皆凱科技".Localize("zh")}");

            //Action actionReplace5 = () =>
            //{
            //    var rgx = new Regex(@"{(.*?)}");
            //    var parts = rgx.Split("Hi, I'm {MarkSix}, this is {System}, the game is {PK10}");
            //    for (var i = 0; i < 1000000; i++)
            //    {
            //        var sb = new StringBuilder();
            //        foreach (var item in parts)
            //        {
            //            string temp;
            //            if (dicZh.TryGetValue(item, out var v))
            //            {
            //                temp = v;
            //            }
            //            else
            //            {
            //                temp = item;
            //            }
            //            sb.Append(temp);
            //        }
            //        txt = sb.ToString();
            //    }
            //};

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

    /// <summary>
    /// 多語系工具
    /// </summary>
    public static class LocalizationUtil
    {
        /// <summary>
        /// 字典表
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> Languages => _langCache;

        private delegate string delgGetParam(object instance);
        private delegate string delgGetProperty(object instance);
        private static Dictionary<string, delgGetProperty> _delgCacheGetProperty = new Dictionary<string, delgGetProperty>();
        private static Dictionary<string, Dictionary<string, string>> _langCache = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, Dictionary<string, string>> _processedCache = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, delgGetParam> _delgCache = new Dictionary<string, delgGetParam>();

        /// <summary>
        /// 设定语系档
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="langCode"></param>
        public static void AddLanguage(Dictionary<string, string> dic, string langCode) => _langCache.Add(langCode, dic);

        /// <summary>
        /// 加入动态参数
        /// </summary>
        /// <param name="str"></param>
        /// <param name="paramData"></param>
        /// <returns></returns>
        public static string AddParams(this string str, object paramData)
        {
            if (!_delgCacheGetProperty.TryGetValue(str, out var lambda))
            {
                lock (_delgCacheGetProperty)
                {
                    if (!_delgCacheGetProperty.TryGetValue(str, out lambda))
                    {
                        var exprList = new List<Expression>();
                        var targetExpr = Expression.Parameter(typeof(object), "target");
                        var memberExpr = Expression.Convert(targetExpr, paramData.GetType());
                        var pipeExpr = Expression.Constant("|");
                        var props = paramData.GetType().GetProperties();
                        var sb = new StringBuilder();
                        exprList.Add(Expression.Constant(str));
                        exprList.Add(pipeExpr);
                        foreach (var prop in props)
                        {
                            var propExpression = Expression.Property(memberExpr, prop);
                            var constExpression = Expression.Constant(propExpression.Member.Name);
                            exprList.Add(constExpression);
                            exprList.Add(pipeExpr);
                            exprList.Add(propExpression);
                            exprList.Add(pipeExpr);
                        }

                        var method = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });
                        var paramsExpr = Expression.NewArrayInit(typeof(object), exprList);
                        var methodExpr = Expression.Call(method, paramsExpr);
                        var lambdaExpr = Expression.Lambda<delgGetProperty>(methodExpr, targetExpr);
                        lambda = lambdaExpr.Compile();
                        _delgCacheGetProperty.Add(str, lambda);
                    }
                }
            }

            return $"##{lambda.Invoke(paramData)}";

            //var param = JsonConvert.SerializeObject(new { Key = str.Replace("{", "").Replace("}", ""), Data = paramData });
            //return $"##{param}";
        }

        /// <summary>
        /// 转换为多语系
        /// </summary>
        /// <param name="str"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string Localize(this string str, string lang)
        {
            var paramModel = default(Dictionary<string, string>);
            if (str.StartsWith("##"))
            {
                var obj = str.Substring(2).Split('|');
                str = obj[0];
                paramModel = new Dictionary<string, string>();
                for (int i = 1; i < obj.Length - 1; i += 2)
                {
                    paramModel.Add(obj[i], obj[i + 1]);
                }
                return Parser(str, lang, paramModel);
            }

            if (!_processedCache.TryGetValue(lang, out var langDic))
            {
                lock (_processedCache)
                {
                    if (!_processedCache.TryGetValue(lang, out langDic))
                    {
                        langDic = new Dictionary<string, string>();
                        _processedCache.Add(lang, langDic);
                    }
                }
            }

            if (!langDic.TryGetValue(str, out var result))
            {
                lock (_processedCache)
                {
                    if (!langDic.TryGetValue(str, out result))
                    {
                        result = Parser(str, lang, paramModel);
                        langDic.Add(str, result);
                    }
                }
            }
            return result;
        }

        private static string Parser(this string str, string lang, object paramModel)
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
                        sb.Append(_langCache[lang][key.ToString()].Parser(lang, paramModel));
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

        private static delgGetParam ProcessParam(string str, object paramModel)
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

                        var keyExpr = Expression.Constant(str);
                        PropertyInfo indexer = (from p in memberExpr.Type.GetDefaultMembers().OfType<PropertyInfo>()
                                                where p.PropertyType == typeof(string)
                                                let q = p.GetIndexParameters()
                                                where q.Length == 1 && q[0].ParameterType == typeof(string)
                                                select p).Single();
                        IndexExpression indexExpr = Expression.Property(memberExpr, indexer, keyExpr);
                        exprList.Add(indexExpr);

                        key.Clear();
                        sb.Clear();
                        start = false;
                        isParam = false;

                        var method = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });
                        var paramsExpr = Expression.NewArrayInit(typeof(object), exprList);
                        var methodExpr = Expression.Call(method, paramsExpr);
                        var lambdaExpr = Expression.Lambda<delgGetParam>(methodExpr, targetExpr);
                        lambda = lambdaExpr.Compile();
                        _delgCache.Add(str, lambda);
                    }
                }
            }

            return lambda;
        }
    }
}

