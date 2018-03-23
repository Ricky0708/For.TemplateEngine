using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace For.TemplateParser
{
    internal class Core
    {
        private delegate object delgGetProperty(object instance);
        private static Regex regex = new Regex("(?<={.)(.*?)(?=})");

        internal static IEnumerable<PropertyInfo> GetProps(object obj, IEnumerable<string> usedPropNames)
        {
            var props = obj.GetType().GetProperties().Where(p => usedPropNames.ToList().Contains(p.Name));
            return props;
        }

        internal static IEnumerable<string> GetUsedPropertyName(string template)
        {
            if (!Caches.IsExist(CacheType.UsedPropertyName, template))
            {
                Caches.Lock(CacheType.UsedPropertyName);
                if (!Caches.IsExist(CacheType.UsedPropertyName, template))
                {
                    try
                    {
                        Caches.Add(CacheType.UsedPropertyName, template, _GetUsedPropertyName(template).ToList());
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Caches.Unlock(CacheType.UsedPropertyName);
                    }
                }
            }
            var result = (IEnumerable<string>)Caches.GetValue(CacheType.UsedPropertyName, template);
            return result;
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
                        Caches.Add(CacheType.GetPropertyValue, keyName, Core._BuildGetPropertyMethod(type, prop));
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

        private static IEnumerable<string> _GetUsedPropertyName(string template)
        {
            var matched = regex.Match(template);
            while (matched.Success)
            {
                yield return matched.Value;
                matched = matched.NextMatch();
            }
        }

        private static delgGetProperty _BuildGetPropertyMethod(Type type, PropertyInfo prop)
        {
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            MemberExpression propertyExp = Expression.Property(Expression.Convert(targetExp, type), prop);

            LambdaExpression lambdax = Expression.Lambda(typeof(delgGetProperty), Expression.Convert(propertyExp, typeof(object)), targetExp);
            delgGetProperty delg = (delgGetProperty)lambdax.Compile();
            return delg;
        }
    }
}
