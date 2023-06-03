﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
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
            TestDForDemo();
        }

        static void TestA()
        {
            var template = "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            template += "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            template += "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            var provider = new TemplateEngine(new For.TemplateEngine.Models.TemplateEngineConfig()
            {
                DateTimeFormat = "yyyyMMdd",
                DateTimeOffsetFormat = "yyyy/MM/dd"
            });
            provider.RegisterTemplate<TestModel>(template);
            var obj = new TestModel()
            {
                Name = "Ricky",
                Age = 25,
                StandardDateTime = DateTime.Parse("2017/08/01"),
                OffsetDateTime = DateTimeOffset.Parse("2017/08/02"),
                Details = new Detail()
                {
                    Id = 0,
                    Mother = new Parent()
                    {
                        Name = "Mary",
                        Age = 50
                    },
                    Father = new Parent()
                    {
                        Name = "Eric",
                        Age = 51
                    }
                }
            };

            Action parallerRender = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    var resultA = provider.Render(obj, typeof(TestModel).FullName);
                    if (!resultA.StartsWith("Hi!"))
                    {
                        throw new Exception();
                    }
                    obj.Age += 1;
                });
            };
            Action seqRender = () =>
            {
                for (var i = 0; i < 1000000; i++)
                {
                    var resultA = provider.Render(obj);
                    obj.Age += 1;
                }
            };
            Action dynamicParallerRender = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    var resultA = provider.DynamicRender(new
                    {
                        Name = "Ricky",
                        Age = 25,
                        StandardDateTime = DateTime.Parse("2017/08/01"),
                        OffsetDateTime = DateTimeOffset.Parse("2017/08/02"),
                        Details = new Detail()
                        {
                            Id = 0,
                            Mother = new Parent()
                            {
                                Name = "Mary",
                                Age = 50
                            },
                            Father = new Parent()
                            {
                                Name = "Eric",
                                Age = 51
                            }
                        }
                    }, template, "dynamicParallerRender");
                    if (!resultA.StartsWith("Hi!"))
                    {
                        throw new Exception();
                    }
                    obj.Age += 1;
                });
            };
            Action dynamicSeqRender = () =>
            {
                for (var i = 0; i < 1000000; i++)
                {
                    var resultA = provider.DynamicRender(new
                    {
                        Name = "Ricky",
                        Age = 25,
                        StandardDateTime = DateTime.Parse("2017/08/01"),
                        OffsetDateTime = DateTimeOffset.Parse("2017/08/02"),
                        Details = new Detail()
                        {
                            Id = 0,
                            Mother = new Parent()
                            {
                                Name = "Mary",
                                Age = 50
                            },
                            Father = new Parent()
                            {
                                Name = "Eric",
                                Age = 51
                            }
                        }
                    }, template, "dynamicSeqRender");
                    obj.Age += 1;
                }
            };

            Watch("parallerRender", parallerRender);
            Watch("seqRender", seqRender);
            Watch("dynamicParallerRender", dynamicParallerRender);
            Watch("dynamicSeqRender", dynamicSeqRender);
            Console.WriteLine(provider.Render(obj));
            Console.ReadLine();
            Main(null);
        }

        static void TestB()
        {
            var templateZh = "Hi! {.Name}, 你的年紀是 {.Age}, {.StandardDateTime}, {.OffsetDateTime}, 你父母的名字是{.Details.Father.Name}, {.Details.Mother.Name}";
            var templateEn = "Hi! {.Name}, your father name is {.Details.Father.Name}, {.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            var templateQQ = "{.Name}-{.Details.Father.Name}#{.Details.Mother.Name}[{.Age}] {.StandardDateTime}%{.OffsetDateTime}";

            var dic = new Dictionary<string, string>();
            dic.Add("zh", templateZh);
            dic.Add("en", templateEn);
            dic.Add("qq", templateQQ);

            var provider = new TemplateEngine(new For.TemplateEngine.Models.TemplateEngineConfig()
            {
                DateTimeFormat = "yyyyMMdd",
                DateTimeOffsetFormat = "yyyy/MM/dd"
            });

            var dataModel = new
            {
                Name = "Ricky",
                Age = 25,
                StandardDateTime = DateTime.Parse("2017/08/01"),
                OffsetDateTime = DateTimeOffset.Parse("2017/08/02"),
                Details = new Detail()
                {
                    Id = 0,
                    Mother = new Parent()
                    {
                        Name = "Mary",
                        Age = 50
                    },
                    Father = new Parent()
                    {
                        Name = "Eric",
                        Age = 51
                    }
                }
            };

            Console.WriteLine(provider.DynamicRender(dataModel, dic["zh"], "dynamicParallerRender"));
            Console.WriteLine("\r\n");
            Console.WriteLine(provider.DynamicRender(dataModel, dic["en"], "dynamicParallerRender1"));
            Console.WriteLine("\r\n");
            Console.WriteLine(provider.DynamicRender(dataModel, dic["qq"], "dynamicParallerRenderqq"));




            //Action dynamicParallerRender = () =>
            //{
            //    Parallel.For((long)0, 1000000, p =>
            //    {
            //        var resultA = templateZh.Replace("{.Name}", dic["{.Name}"]);
            //        if (!resultA.StartsWith("Hi!"))
            //        {
            //            throw new Exception();
            //        }
            //    });
            //};

            //Action dynamicParallerRenderB = () =>
            //{
            //    Parallel.For((long)0, 1000000, p =>
            //    {
            //        var resultA = provider.DynamicRender(new
            //        {
            //            Name = "Ricky",
            //            Age = 25,
            //            StandardDateTime = DateTime.Parse("2017/08/01"),
            //            OffsetDateTime = DateTimeOffset.Parse("2017/08/02"),
            //            Details = new Detail()
            //            {
            //                Id = 0,
            //                Mother = new Parent()
            //                {
            //                    Name = "Mary",
            //                    Age = 50
            //                },
            //                Father = new Parent()
            //                {
            //                    Name = "Eric",
            //                    Age = 51
            //                }
            //            }
            //        }, templateZh, "dynamicParallerRender");
            //        if (!resultA.StartsWith("Hi!"))
            //        {
            //            throw new Exception();
            //        }
            //    });
            //};

            //Watch("dynamicParallerRender", dynamicParallerRender);
            //Watch("dynamicParallerRenderB", dynamicParallerRenderB);
            Console.ReadLine();
            Main(null);
        }

        static void TestC()
        {
            var sentenceKey = "sentenceKey";
            var profileNameKey = "ProfileName";
            var profileAgeKey = "ProfileAge";
            var profileStartDateTimeKey = "ProfileStartDateTimeKey";
            var profileEndDateTimeKey = "ProfileEndDateTimeKey";
            var profileFatherNameKey = "Details.Father.Name";
            var profileMotherNameKey = "Details.Mother.Name";

            var dic = new Dictionary<string, string>();
            dic.Add(sentenceKey + "_zh", "Hi! {ProfileName}, 你的年紀是 {#ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}, 你父母的名字是{Details.Father.Name}, {Details.Mother.Name}");
            dic.Add(profileNameKey + "_zh", "{MyA}{MyB}");
            dic.Add("MyA_zh", "{#AA}");
            dic.Add("MyB_zh", "{MyA}{MyC}");
            dic.Add("MyC_zh", "{#MyC}");
            dic.Add(profileAgeKey + "_zh", "20");
            dic.Add(profileStartDateTimeKey + "_zh", "2023/05/31");
            dic.Add(profileEndDateTimeKey + "_zh", "2023/06/01");
            dic.Add(profileFatherNameKey + "_zh", "爸爸");
            dic.Add(profileMotherNameKey + "_zh", "媽媽");
            dic.Add(sentenceKey + "_en", "Hi! {ProfileName}, your father name is {Details.Father.Name}, {Details.Mother.Name}, your age is {ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}");
            dic.Add(profileNameKey + "_en", "Ricky");
            dic.Add(profileAgeKey + "_en", "30");
            dic.Add(profileStartDateTimeKey + "_en", "2023/06/02");
            dic.Add(profileEndDateTimeKey + "_en", "2023/06/03");
            dic.Add(profileFatherNameKey + "_en", "Father");
            dic.Add(profileMotherNameKey + "_en", "Mother");
            Extension.SetCache(dic, "zh");

            var a = "";
            var b = "";

            Action parallerRender = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    //a = "Hi! {ProfileName}, 你的年紀是 {ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}, 你父母的名字是{Details.Father.Name}, {Details.Mother.Name}".ConvertToResult(dic, "zh");
                    a = dic[sentenceKey + "_zh"].ConvertToResult("zh", new { ProfileAge = "20", AA = "AAA", BB = "BBB", MyC = "CCC" });
                    //b = dic[sentenceKey + "_en"].ConvertToResult(dic, "en");
                });
            };

            Action parallerRenderB = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    a = "Hi! {ProfileName}, 你的年紀是 {ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}, 你父母的名字是{Details.Father.Name}, {Details.Mother.Name}"
                    .Replace("{profileNameKey}", dic[profileNameKey + "_zh"])
                    .Replace("{profileAgeKey}", "20")
                    .Replace("{profileStartDateTimeKey}", dic[profileStartDateTimeKey + "_zh"])
                    .Replace("{profileEndDateTimeKey}", dic[profileEndDateTimeKey + "_zh"])
                    .Replace("{profileFatherNameKey}", dic[profileFatherNameKey + "_zh"])
                    .Replace("{profileMotherNameKey}", dic[profileMotherNameKey + "_zh"]);
                });
            };
            Action parallerRenderC = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    a = string.Format("Hi! {0}, 你的年紀是 {1}, {2}, {3}, 你父母的名字是{4}, {5}"
                    , dic[profileNameKey + "_zh"]
                    , 20
                    , dic[profileStartDateTimeKey + "_zh"]
                    , dic[profileEndDateTimeKey + "_zh"]
                    , dic[profileFatherNameKey + "_zh"]
                    , dic[profileMotherNameKey + "_zh"]);
                });
            };
            Watch("parallerRender", parallerRender);
            Watch("parallerRenderB", parallerRenderB);
            Watch("parallerRenderC", parallerRenderC);

            a = dic[sentenceKey + "_zh"].ConvertToResult("zh", new { ProfileAge = "60", AA = "AAA", BB = "BBB", MyC = "CCC" });
            b = dic[sentenceKey + "_en"].ConvertToResult("en");

            Console.Write("\r\n\r\n");
            Console.Write(a);
            Console.Write("\r\n\r\n");
            Console.Write(b);
            Console.ReadLine();
        }

        static void TestDForDemo()
        {
            #region Init dic

            var sentenceKey = "sentenceKey";
            var profileNameKey = "ProfileName";
            var profileAgeKey = "ProfileAge";
            var profileStartDateTimeKey = "ProfileStartDateTimeKey";
            var profileEndDateTimeKey = "ProfileEndDateTimeKey";
            var profileFatherNameKey = "Details.Father.Name";
            var profileMotherNameKey = "Details.Mother.Name";

            var dicZh = new Dictionary<string, string>();
            var dicEn = new Dictionary<string, string>();
            dicZh.Add(sentenceKey, "Hi! {ProfileName}, 你的年紀是 {#ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}, 你父母的名字是{Details.Father.Name}, {Details.Mother.Name}");
            dicZh.Add(profileNameKey, "{MyA}{MyB}");
            dicZh.Add("MyA", "{#AA}");
            dicZh.Add("MyB", "{MyA}{MyC}");
            dicZh.Add("MyC", "{#MyC}");
            dicZh.Add(profileAgeKey, "20");
            dicZh.Add(profileStartDateTimeKey, "2023/05/31");
            dicZh.Add(profileEndDateTimeKey, "2023/06/01");
            dicZh.Add(profileFatherNameKey, "爸爸");
            dicZh.Add(profileMotherNameKey, "媽媽");

            dicEn.Add(sentenceKey, "Hi! {ProfileName}, your father name is {Details.Father.Name}, {Details.Mother.Name}, your age is {ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}");
            dicEn.Add(profileNameKey, "{MyA}{MyB}");
            dicEn.Add("MyA", "tYY");
            dicEn.Add("MyB", "{MyA}{MyC}");
            dicEn.Add("MyC", "SDFE");
            dicEn.Add(profileAgeKey, "30");
            dicEn.Add(profileStartDateTimeKey, "2023/06/02");
            dicEn.Add(profileEndDateTimeKey, "2023/06/03");
            dicEn.Add(profileFatherNameKey, "Father");
            dicEn.Add(profileMotherNameKey, "Mother");
            Extension.SetCache(dicZh, "zh");
            Extension.SetCache(dicEn, "en");

            #endregion

            var a = "";
            var b = "";
            var json = JsonConvert.SerializeObject(new { ProfileAge = "60", AA = "AAA", BB = "BBB", MyC = "CCC" });
            var jb = JsonConvert.DeserializeObject<JObject>(json);

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
                    a = Extension.GetMessage(sentenceKey, "en");
                });
            };

            Watch("parallerRenderA 有動態參數", parallerRenderA);
            Watch("parallerRenderB 無動態參數", parallerRenderB);

            a = Extension.GetMessage(sentenceKey, "zh", jb);
            b = Extension.GetMessage(sentenceKey, "en");
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

        public static void SetCache(Dictionary<string, string> dic, string langCode) => _cache.Add(langCode, dic);

        public static string GetMessage(string code, string langCode, object paramModel = null)
        {
            if (_cache.TryGetValue(langCode, out var langDic))
            {
                if (langDic.TryGetValue(code, out var result))
                {
                    if (paramModel == null)
                    {
                        return result.ConvertToResult(langCode);
                    }
                    else
                    {
                        return result.ConvertToResult(langCode, paramModel);
                    }
                }
            }
            return "";
        }

        public static string ConvertToResult(this string str, string lang)
        {
            return str.ProcessString(lang);
        }

        public static string ConvertToResult(this string str, string lang, object paramModel)
        {
            var resultString = ProcessString(str, lang);
            return ProcessParam(resultString, paramModel).Invoke(paramModel);
        }

        private static string ProcessString(this string str, string lang)
        {
            var sb = new StringBuilder();
            var start = false;
            var isParam = false;
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
                                if (isParam)
                                {
                                    sb.Append($"{{{key}}}");
                                }
                                else
                                {
                                    sb.Append(_cache[lang][key.ToString()].ProcessString(lang));
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
                        _cache[lang].Add(str, result);
                    }
                }
            }

            return result;
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

                                var jMethodSelectToken = typeof(JToken).GetMethod("SelectTokens", new[] { typeof(string) });
                                var jMethodFirst = typeof(Enumerable).GetMethods().Where(p => p.Name == "First" && p.GetParameters().Count() == 1).First();
                                var jMethodValue = typeof(JToken).GetMethods().Where(m =>
                                        m.Name == "Value"
                                        ).First();
                                var mx = Expression.Call(memberExpr, jMethodSelectToken, Expression.Constant(key.ToString()));
                                var mx2 = Expression.Call(typeof(Enumerable), "First", new Type[] { typeof(JToken) }, mx);
                                var mx3 = Expression.Call(typeof(Newtonsoft.Json.Linq.Extensions), "Value", new Type[] { typeof(string) }, mx2);

                                exprList.Add(mx3);
                                //exprList.Add(Expression.Property(memberExpr, key.ToString()));
                                key.Clear();
                                sb.Clear();
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

    }
}
