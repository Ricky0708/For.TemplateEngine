using For.TemplateParser;
using System;
using System.Collections.Generic;
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
            var template = "Hi! {.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}{.QQ}";
            var obj = new TestModel()
            {
                Name = "Ricky",
                Age = 25,
                StandardDateTime = DateTime.Parse("2017/08/01"),
                OffsetDateTime = DateTimeOffset.Parse("2017/08/02")
            };
            var resultA = TemplateParserProvider.GetLogString(obj, template);
            var resultB = TemplateParserProvider.GetLogString(obj, p => $"Hi! {p.Name}, your age is {p.Age}, {p.StandardDateTime}, {p.OffsetDateTime}");
            var resultC = TemplateParserProvider.GetLogString(new
            {
                Name = "Ricky",
                Age = 25,
                StandardDateTime = DateTime.Parse("2017/08/01"),
                OffsetDateTime = DateTimeOffset.Parse("2017/08/02")
            }, template);

            Console.WriteLine(resultA);
            Console.WriteLine(resultB);
            Console.WriteLine(resultC);
            Console.ReadLine();
            //var usedPropertyName = GetUsedPropertyName(template);
            //var props = GetProps(obj, usedPropertyName);
            //GetPropValue(obj, props.First());
        }
    }
}
