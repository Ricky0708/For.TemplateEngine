using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace For.TemplateParser
{
    public class TemplateParserProvider
    {

        /// <summary>
        /// 組合物件與範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static string BuildTemplate<T>(T obj, string template)
        {
            var usedPropertyName = Core.GetUsedPropertyName(template);
            var props = Core.GetProps(obj, usedPropertyName);
     
            foreach (var prop in props)
            {
                template = template.Replace($"{{.{prop.Name}}}", Core.GetPropValue(obj, prop)?.ToString() ?? "#empty#");
            }
            return template;
        }
        /// <summary>
        /// 組合物件與範本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static string BuildTemplate<T>(T obj, Func<T, string> func)
        {
            return func(obj);
        }
    }
}
