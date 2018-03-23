using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TemplateLogEngine
{
    internal class Core
    {
        internal delegate object delgGetProperty(object instance);

        internal static IEnumerable<PropertyInfo> GetProps(object obj, IEnumerable<string> usedPropNames)
        {
            var props = obj.GetType().GetProperties().Where(p => usedPropNames.ToList().Contains(p.Name));
            return props;
        }

        internal static IEnumerable<string> GetUsedPropertyName(string template)
        {
            Regex regex = new Regex("(?<={.)(.*?)(?=})");
            var matched = regex.Match(template);
            while (matched.Success)
            {
                yield return matched.Value;
                matched = matched.NextMatch();
            }
        }

        internal static object GetPropValue<T>(T instance, PropertyInfo prop)
        {
            Type type = instance.GetType();
            var typeName = type.FullName;
            var keyName = typeName + prop.PropertyType.Name + prop.Name + "_Get";
            if (!Caches.IsExist(CacheType.GetPropertyValue, keyName))
            {
                Caches.Lock(CacheType.GetPropertyValue);
                if (!Caches.IsExist(CacheType.GetPropertyValue, keyName))
                {
                    try
                    {
                        Caches.Add(CacheType.GetPropertyValue, keyName, Core.BuildGetPropertyMethod(type, prop));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Caches.Unlock(CacheType.GetPropertyValue);
                    }
                }
            }
            delgGetProperty GetPropertyAction = (delgGetProperty)Caches.GetValue(CacheType.GetPropertyValue, keyName);
            return GetPropertyAction(instance);
        }

        private static delgGetProperty BuildGetPropertyMethod(Type type, PropertyInfo prop)
        {
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            MemberExpression propertyExp = Expression.Property(Expression.Convert(targetExp, type), prop);

            LambdaExpression lambdax = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(propertyExp, typeof(object)), targetExp);
            delgGetProperty delg = (delgGetProperty)lambdax.Compile();
            return delg;
        }
    }
}
