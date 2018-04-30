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
            var provider = new TemplateParser();
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

            //for (int i = 0; i < 1000000; i++)
            //{
            //    var n = $"Hi! {obj.Name}, your father name is [#{obj.Details.Father.Name}[#{obj.Details.Mother.Name}#]#], your age is {obj.Age}, {obj.StandardDateTime}, {obj.OffsetDateTime}" +
            //        $"Hi! {obj.Name}, your father name is [#{obj.Details.Father.Name}[#{obj.Details.Mother.Name}#]#], your age is {obj.Age}, {obj.StandardDateTime}, {obj.OffsetDateTime}" +
            //        $"Hi! {obj.Name}, your father name is [#{obj.Details.Father.Name}[#{obj.Details.Mother.Name}#]#], your age is {obj.Age}, {obj.StandardDateTime}, {obj.OffsetDateTime}";
            //}

            for (int i = 0; i < 1000000; i++)
            {
                var n = "Hi! {.Name}, your father name is [#{.Details.Father.Name}[#{.Details.Mother.Name}#]#], your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}" +
                    "Hi! {.Name}, your father name is [#{.Details.Father.Name}[#{.Details.Mother.Name}#]#], your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}" +
                    "Hi! {.Name}, your father name is [#{.Details.Father.Name}[#{.Details.Mother.Name}#]#], your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}"
                    .Replace("{.Name}", obj.Name)
                    .Replace("{.Details.Father.Name}", obj.Details.Father.Name)
                    .Replace("{.Details.Mother.Name}", obj.Details.Mother.Name)
                    .Replace("{.Age}", obj.Age.ToString())
                    .Replace("{.StandardDateTime}", obj.StandardDateTime.ToString())
                    .Replace("{.OffsetDateTime}", obj.OffsetDateTime.ToString());
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch.Reset();
            watch.Start();
            for (int i = 0; i < 1000000; i++)
            {
                var resultA = provider.BuildTemplate(obj, template);
                obj.Age += 1;
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            Parallel.For((long)0, 1000000, p =>
            {
                var resultA = provider.BuildTemplate(obj, template);
                //TemplateParser.ClearCaches();
                if (!resultA.StartsWith("Hi!"))
                {
                    throw new Exception();
                }
                obj.Age += 1;
            });
            //var resultB = TemplateParser.BuildTemplate(obj, p => $"Hi! {p.Name}, your age is {p.Age}, {p.StandardDateTime}, {p.OffsetDateTime}");
            //var resultC = TemplateParser.BuildTemplate(
            //    new
            //    {
            //        Name = "Ricky",
            //        Age = 25,
            //        StandardDateTime = DateTime.Parse("2017/08/01"),
            //        OffsetDateTime = DateTimeOffset.Parse("2017/08/02")
            //    }, template);

            //Console.WriteLine(resultA);
            //Console.WriteLine(resultB);
            //Console.WriteLine(resultC);
            Console.ReadLine();
            //var usedPropertyName = GetUsedPropertyName(template);
            //var props = GetProps(obj, usedPropertyName);
            //GetPropValue(obj, props.First());
        }
    }
}
