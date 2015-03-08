﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace OdbcWrapper
{
    public static class DataExtensions
    {
        internal static T Get<T>(this System.Data.DataRow row, string columnName)
        {
            var index = row.Table.Columns.IndexOf(columnName);
            var value = row.ItemArray[index];
            var result = SafeConvert<T>(value);
            return result;
        }


        public static T Get<T>(this DataRow row, string columnName)
        {
            var index = row.Table._schemaTable.Columns.IndexOf(columnName);
            var value = row._dataRecord.GetValue(index);
            var result = SafeConvert<T>(value);
            return result;
        }



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
                    var value = dict[key];
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
                if (typeof (IConvertible).IsAssignableFrom(typeof (T)))
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

    }
}
