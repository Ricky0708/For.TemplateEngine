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
        /// <summary>
        /// 取得property的委派型別
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private delegate object delgGetProperty(object instance);
        /// <summary>
        /// 由範本中抽取特殊標記的pattern
        /// </summary>
        private static Regex regex = new Regex("(?<={.)(.*?)(?=})");

        /// <summary>
        /// 取出物件中被使用到的property清單
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="usedPropNames"></param>
        /// <returns></returns>
        internal static IEnumerable<PropertyInfo> GetProps(object obj, IEnumerable<string> usedPropNames)
        {
            var props = obj.GetType().GetProperties().Where(p => usedPropNames.ToList().Contains(p.Name));
            return props;
        }

        /// <summary>
        /// 取出範本中的特殊標記的清單
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 使用 expression 取出物件中property的value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
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
