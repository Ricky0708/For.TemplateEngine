using For.TemplateParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var template = "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            template += "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            template += "Hi! {.Name}, your father name is {.Details.Father.Name}{.Details.Mother.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            var provider = new TemplateParser(new For.TemplateParser.Models.TemplateParserConfig()
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

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Parallel.For((long)0, 10000, p =>
            {
                var resultA = provider.Render(obj, typeof(TestModel).FullName);
                if (!resultA.StartsWith("Hi!"))
                {
                    throw new Exception();
                }
                obj.Age += 1;
            });

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch.Reset();
            watch.Start();
            for (var i = 0; i < 1000000; i++)
            {
                var resultA = provider.Render(obj);
                obj.Age += 1;
            }
            watch.Stop();
            Console.WriteLine(provider.Render(obj));
            Console.WriteLine(watch.ElapsedMilliseconds);

         
            Console.ReadLine();
           
        }
    }
}
