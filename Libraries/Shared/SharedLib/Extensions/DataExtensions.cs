using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace SharedLib
{
    public static class DataExtensions
    {
        public static T Get<T>(this ExpandoObject expando, string key)
        {
            if (expando == null)
                return default(T);

            var dict = ((IDictionary<string, object>) expando);
            T result;
            if (dict.ContainsKey(key))
            {
                var obj = dict[key];
                result = SafeConvert<T>(obj);
            }
            else
                result = default(T);
            return result;
        }


        public static void Set(this ExpandoObject expando, string key, object value)
        {
            var dict = ((IDictionary<string, object>)expando);
            dict[key] = value;
        }
        

        public static bool Remove(this ExpandoObject expando, string key)
        {
            var dict = ((IDictionary<string, object>)expando);
            var res = dict.Remove(key);
            return res;
        }


        public static ExpandoObject Extend(this ExpandoObject expando, ExpandoObject additional)
        {
            foreach (var pair in additional)
            {
                expando.Set(pair.Key, pair.Value);
            }
            return expando;
        }



        public static T To<T>(this ExpandoObject expando)
            where T : new()
        {
            if (expando == null)
                return default(T);

            var result = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dict = ((IDictionary<string, object>)expando);
            for (var i = 0; i < dict.Keys.Count; i++)
            {
                var key = dict.Keys.ElementAt(i);
                var prop = properties.SingleOrDefault(x => x.Name == key);
                if (prop != null)
                {
                    var raw = dict[key];
                    //var value = Convert.ChangeType(raw, prop.PropertyType);
                    var value = SafeConvert(raw, prop.PropertyType, null);
                    prop.SetValue(result, value);
                }
            }
            return result;
        }



        public static object Default(this Type type)
        {
            if (type == typeof (string))
                return default (string);
            var obj = Activator.CreateInstance(type);
            return obj;
        }



        public static T SafeConvert<T>(this object value)
        {
            var result = SafeConvert<T>(value, default(T));
            return result;
        }


        public static T SafeConvert<T>(this object value, T defaultValue)
        {
            var result = defaultValue;
            try
            {
                if (value == null)
                    return result;
                if (value == DBNull.Value)
                    return result;
                if (typeof (T) == value.GetType())
                {
                    result = (T) value;
                }
                else if (typeof(T).IsEnum)
                {
                    var str = value.SafeConvert<string>();
                    var obj = Enum.Parse(typeof(T), str);
                    result = (T)obj;
                }
                else if (typeof (IConvertible).IsAssignableFrom(typeof (T)))
                {
                    var obj = Convert.ChangeType(value, typeof (T));
                    result = (T) obj;
                }
                else
                {
                    result = (T) value;
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public static object SafeConvert(this object value, Type targetType, object defaultValue)
        {
            var result = defaultValue;
            try
            {
                if (value == null)
                    return result;
                if (value == DBNull.Value)
                    return result;
                var type = value.GetType();
                if (targetType == type)
                {
                    result = value;
                }
                else if (targetType.IsAssignableFrom(type))
                {
                    result = value;
                }
                else if (targetType.IsEnum)
                {
                    var str = value.SafeConvert<string>();
                    result = Enum.Parse(targetType, str);
                }
                else if (typeof (IConvertible).IsAssignableFrom(targetType))
                {
                    result = Convert.ChangeType(value, targetType);
                }
                else
                {
                    //result = (T) value;       // todo?
                    throw new NotImplementedException();
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

    }
}
