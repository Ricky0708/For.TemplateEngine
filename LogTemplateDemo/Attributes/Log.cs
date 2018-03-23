using For.TemplateParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LogTemplateDemo.Attributes
{
    public class LogA : ActionFilterAttribute
    {
        private readonly string _template = "";
        public LogA(string template)
        {
            _template = template;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //TemplateParserProvider
            string log = "";
            switch (filterContext.ActionParameters.Count)
            {
                case int x when x > 1: // 多個基本型別
                    var @params = filterContext.ActionParameters;
                    log = GenerateLogFromBasicType(_template, @params);
                    break;
                case int x when x == 1: // 單一型別或是model
                    var param = filterContext.ActionParameters;
                    if (IsSimpleType(filterContext.ActionDescriptor.GetParameters().First().ParameterType))
                    {
                        log = GenerateLogFromBasicType(_template, param);
                    }
                    else
                    {
                        log = TemplateParserProvider.GetLogString(param.First().Value, _template);
                    }
                    break;
                default:
                    log = _template;
                    break;
            }
            // TODO: Log template to log
            base.OnActionExecuting(filterContext);
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
        private string GenerateLogFromBasicType(string template, IDictionary<string, object> @params)
        {
            foreach (var param in @params)
            {
                template = template.Replace($"{{.{param.Key}}}", param.Value?.ToString() ?? "#empty# ");
            }
            return template;
        }
        private bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimpleType(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
    }
}
