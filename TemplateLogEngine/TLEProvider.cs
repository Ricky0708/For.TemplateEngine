using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TemplateLogEngine
{
    public class TLEProvider
    {
        public static string GetLogString<T>(T obj, string template)
        {
            var usedPropertyName = Core.GetUsedPropertyName(template);
            var props = Core.GetProps(obj, usedPropertyName);
            foreach (var prop in props)
            {
                template = template.Replace($"{{.{prop.Name}}}", Core.GetPropValue(obj, prop).ToString());
            }
            return template;
        }
        public static string GetLogString<T>(T obj, Func<T,string> func)
        {
            return func(obj);
        }
    }
}
