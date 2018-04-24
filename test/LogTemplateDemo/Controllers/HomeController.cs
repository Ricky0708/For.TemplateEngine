using LogTemplateDemo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogTemplateDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [LogA("Hello {.a}{.b}")]
        public string Test(string a, string b)
        {
            return "";
        }

        [LogA("Hello {.c}{.b}")]
        public string Test2(string c)
        {
            return "";
        }

        [LogA("Hello {.a}{.b}")]
        public string Test3()
        {
            return "";
        }
        [LogA("Hello {.Astring}{.Bint}{.Cint}{.Ddate}{.Edate}{.QQ}")]
        public string Test4(TestLog obj)
        {
            return "";
        }

        [LogA("Hellon kitty {.obj}")]
        public string Test5(int? obj)
        {
            return "";
        }

        [LogA("Hello {.obj}")]
        public string Test6(QQ? obj)
        {
            return "";
        }
    }

    public enum QQ
    {
        A = 1
    }

    public class TestLog
    {
        public string Astring { get; set; }
        public int Bint { get; set; }
        public int? Cint { get; set; }
        public DateTime Ddate { get; set; }
        public DateTime? Edate { get; set; }
        public QQ QQ { get; set; }
    }
}
//http://localhost:9592/Home/test?a=1&b=2
//http://localhost:9592/Home/test2?c=1
//http://localhost:9592/Home/test3
//http://localhost:9592/Home/test4?Astring=1&Bint=2&Cint=3&Ddate=2017/01/01&Edate=2017/02/02&QQ=1
//http://localhost:9592/Home/test5?obj=1
//http://localhost:9592/Home/test6?obj=1