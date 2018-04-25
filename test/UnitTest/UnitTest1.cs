using System;
using System.Diagnostics;
using System.Threading.Tasks;
using For.TemplateParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private string template = "";
        private TemplateParser provider;
        private TestModel model;
        [TestInitialize]
        public void Config()
        {
            template = "Hi! {.Name}, your age is {.Age}, {.StandardDateTime}, {.OffsetDateTime}";
            provider = new TemplateParser();
            model = new TestModel()
            {
                Name = "Ricky",
                Age = 25,
                StandardDateTime = DateTime.Parse("2017/08/01"),
                OffsetDateTime = DateTimeOffset.Parse("2017/08/02")
            };
        }
        [TestMethod]
        public void PerformancTest()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string resultA = "";
            Parallel.For(0, 1000000, p =>
            {
                resultA = provider.BuildTemplate(model, template);
                //TemplateParser.ClearCaches();
                if (!resultA.StartsWith("Hi!"))
                {
                    throw new Exception();
                }
            });
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 2000);
        }

        [TestMethod]
        public void ResultTest()
        {
            string resultA = "";
            resultA = provider.BuildTemplate(model, template);
            Assert.AreEqual(
                resultA,
                "Hi! Ricky, your age is 25, 8/1/2017 12:00:00 AM, 8/2/2017 12:00:00 AM +08:00");
        }
    }
    public class TestModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime StandardDateTime { get; set; }
        public DateTimeOffset OffsetDateTime { get; set; }
    }
}
