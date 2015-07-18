using SharedLib;

namespace OdbcWrapper
{
    public static class DataExtensions
    {
        internal static T Get<T>(this System.Data.DataRow row, string columnName)
        {
            var index = row.Table.Columns.IndexOf(columnName);
            var value = row.ItemArray[index];
            var result = value.SafeConvert<T>();
            return result;
        }


        public static T Get<T>(this DataRow row, string columnName)
        {
            var index = row.Table._schemaTable.Columns.IndexOf(columnName);
            var value = row._dataRecord.GetValue(index);
            var result = value.SafeConvert<T>();
            return result;
        }

    }
}
