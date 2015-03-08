using System;

namespace OdbcWrapper
{
    public class DataColumn
    {
        private readonly DataTable _dataTable;

        internal DataColumn(DataTable dataTable)
        {
            _dataTable = dataTable;

            //ColumnName = column.ColumnName;
            //Caption = column.Caption;
            //DataType = column.DataType;
            //DefaultValue = column.DefaultValue;
            //Expression = column.Expression;
            //AutoIncrement = column.AutoIncrement;
            //AllowDBNull = column.AllowDBNull;
        }


        public DataTable Table { get { return _dataTable; } }

        
        public string ColumnName { get; set; }

        public string Caption { get; set; }
        
        public Type DataType { get; set; }

        public object DefaultValue { get; set; }

        public string Expression { get; set; }

        public bool AutoIncrement { get; set; }

        public bool AllowDBNull { get; set; }

    }
}