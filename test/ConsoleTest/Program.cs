using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using For.TemplateEngine;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestA();
            //TestB();
            TestC();
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
            Extension.SetCache(dic);

            var a = "";
            var b = "";

            Action parallerRender = () =>
            {
                Parallel.For((long)0, 1000000, p =>
                {
                    //a = "Hi! {ProfileName}, 你的年紀是 {ProfileAge}, {ProfileStartDateTimeKey}, {ProfileEndDateTimeKey}, 你父母的名字是{Details.Father.Name}, {Details.Mother.Name}".ConvertToResult(dic, "zh");
                    a = dic[sentenceKey + "_zh"].ConvertToResult("zh", new { ProfileAge = 20, AA = "AAA", BB = "BBB" });
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

            a = dic[sentenceKey + "_zh"].ConvertToResult("zh", new { ProfileAge = 60, AA = "AAA", BB = "BBB" });
            b = dic[sentenceKey + "_en"].ConvertToResult("en");

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

        private static Dictionary<string, string> cache = new Dictionary<string, string>();

        public static void SetCache(Dictionary<string, string> dic) => cache = dic;

        public static string ConvertToResult(this string str, string lang)
        {
            var sb = new StringBuilder();
            var start = false;
            var key = new StringBuilder();
            foreach (var chr in str)
            {
                if (chr == '{')
                {
                    start = true;
                }
                else if (chr == '}')
                {
                    sb.Append(cache[key + $"_{lang}"].ConvertToResult(lang));
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
            return sb.ToString();
        }

        public static string ConvertToResult(this string str, string lang, object paramModel)
        {
            var sb = new StringBuilder();
            var start = false;
            var isParam = false;
            var key = new StringBuilder();
            if (!cache.TryGetValue(str, out var result))
            {
                lock (cache)
                {
                    if (!cache.TryGetValue(str, out result))
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
                                    sb.Append(cache[key + $"_{lang}"].ConvertToResult(lang, paramModel));
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
                        cache.Add(str, result);
                    }
                }
            }

            return result;
            //return ProcessParam(result, paramModel);
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

        private static string ProcessParam(string str, object paramModel)
        {
            var sb = new StringBuilder();
            var start = false;
            var key = new StringBuilder();

            foreach (var chr in str)
            {
                if (chr == '{')
                {
                    start = true;
                }
                else if (chr == '}')
                {
                    var value = paramModel.GetType().GetProperty(key.ToString()).GetValue(paramModel);
                    sb.Append(value);
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
            return sb.ToString();
        }

    }
}
