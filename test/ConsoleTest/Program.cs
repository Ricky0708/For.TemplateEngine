using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
}
