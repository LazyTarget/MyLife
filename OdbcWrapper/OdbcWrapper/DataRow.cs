using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace OdbcWrapper
{
    public class DataRow
    {
        private readonly DataTable _dataTable;
        internal readonly DbDataRecord _dataRecord;

        internal DataRow(DataTable dataTable, DbDataRecord dataRecord)
        {
            _dataTable = dataTable;
            _dataRecord = dataRecord;
        }


        public DataTable Table { get { return _dataTable; } }


        public ExpandoObject ToExpando()
        {
            var expando = new ExpandoObject();
            for (var i = 0; i < _dataRecord.FieldCount; i++)
            {
                var column = _dataTable.Columns.ElementAt(i);
                var caption = column.Caption;
                var value = _dataRecord.GetValue(i);
                expando.Set(caption, value);
            }
            return expando;
        }

        public T To<T>()
            where T : new()
        {
            var result = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (var i = 0; i < _dataRecord.FieldCount; i++)
            {
                var column = _dataTable.Columns.ElementAt(i);
                var caption = column.Caption;
                var value = _dataRecord.GetValue(i);
                var prop = properties.SingleOrDefault(x => x.Name == caption);
                if (prop != null)
                    prop.SetValue(result, value);
            }
            return result;
        }

    }
}