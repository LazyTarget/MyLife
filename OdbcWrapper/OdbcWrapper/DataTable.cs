using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace OdbcWrapper
{
    public class DataTable
    {
        private readonly DbDataReader _dataReader;
        private List<DataColumn> _columns = new List<DataColumn>();
        private List<DataRow> _rows = new List<DataRow>();
        internal System.Data.DataTable _schemaTable;

        public DataTable()
        {

        }

        public DataTable(DbDataReader dataReader)
            : this()
        {
            _dataReader = dataReader;
            Read();
        }


        
        public IEnumerable<DataColumn> Columns
        {
            get { return _columns; }
        }

        public IEnumerable<DataRow> Rows
        {
            get { return _rows; }
        }

        public int RowCount
        {
            get { return _rows.Count; }
        }



        private void Read()
        {
            _rows = new List<DataRow>();
            _columns = new List<DataColumn>();

            _schemaTable = _dataReader.GetSchemaTable();
            for (var i = 0; i < _schemaTable.Rows.Count; i++)
            {
                var row = _schemaTable.Rows.Cast<System.Data.DataRow>().ElementAt(i);
                var c = new DataColumn(this);

                c.ColumnName = row.Get<string>("BaseColumnName");
                if (string.IsNullOrEmpty(c.ColumnName))
                    c.ColumnName = row.Get<string>("ColumnName");
                c.Caption = row.Get<string>("ColumnName");
                if (string.IsNullOrEmpty(c.Caption))
                    c.Caption = row.Get<string>("BaseColumnName");

                c.AllowDBNull = row.Get<bool>("AllowDBNull");
                c.DataType = row.Get<Type>("DataType");
                c.DefaultValue = c.DataType.Default();
                _columns.Add(c);
            }

            foreach (DbDataRecord record in _dataReader)
            {
                var row = new DataRow(this, record);
                _rows.Add(row);
            }

        }

    }
}
