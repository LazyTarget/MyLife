using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;

namespace OdbcWrapper
{
    public class OdbcCommand2
    {
        private readonly System.Data.Odbc.OdbcConnection _connection;

        internal OdbcCommand2(OdbcConnection connection)
        {
            _connection = connection;
            Parameters = new List<OdbcParameter2>();
        }


        public string CommandText { get; set; }

        public List<OdbcParameter2> Parameters { get; private set; }


        public void AddParam(string name, object value)
        {
            var p = new OdbcParameter2
            {
                Name = name,
                Value = value,
            };
            Parameters.Add(p);
        }

        private OdbcCommand ToOdbcCommand()
        {
            var cmd = new OdbcCommand();
            cmd.Connection = _connection;
            cmd.CommandText = CommandText;
            foreach (var p in Parameters)
            {
                var x = new OdbcParameter(p.Name, p.Value);
                if (p.IsOutput)
                {
                    x.Direction = ParameterDirection.Output;
                    if (p.Size.HasValue)
                        x.Size = p.Size.Value;
                    else
                        x.Size = 32;
                }
                else
                {
                    if (p.Size.HasValue)
                        x.Size = p.Size.Value;
                }
                cmd.Parameters.Add(x);
            }
            return cmd;
        }

        
        public async Task<int> ExecuteNonQuery()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var cmd = ToOdbcCommand();
            var result = await cmd.ExecuteNonQueryAsync();

            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                var p = cmd.Parameters.Cast<OdbcParameter>().ElementAt(i);
                if (p.Direction == ParameterDirection.Output)
                {
                    Parameters[i].Value = p.Value;
                }
            }
            return result;
        }
        
        public async Task<DataTable> ExecuteReader()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var cmd = ToOdbcCommand();
            var reader = await cmd.ExecuteReaderAsync();
            var result = new DataTable(reader);
            return result;
        }
        
        public async Task<object> ExecuteScalar()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var cmd = ToOdbcCommand();
            var result = await cmd.ExecuteScalarAsync();
            return result;
        }

    }
}
