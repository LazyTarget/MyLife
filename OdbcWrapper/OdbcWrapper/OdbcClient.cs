using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;

namespace OdbcWrapper
{
    public class OdbcClient
    {
        private readonly OdbcConnection _connection;

        public OdbcClient(string connectionString)
        {
            _connection = new OdbcConnection(connectionString);
        }

        
        public async Task<int> ExecuteNonQuery(string sql)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var cmd = new OdbcCommand(sql, _connection);
            var result = await cmd.ExecuteNonQueryAsync();
            return result;
        }
        
        public async Task<DataTable> ExecuteReader(string sql)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var cmd = new OdbcCommand(sql, _connection);
            var reader = await cmd.ExecuteReaderAsync();
            var result = new DataTable(reader);
            return result;
        }
        
        public async Task<object> ExecuteScalar(string sql)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var cmd = new OdbcCommand(sql, _connection);
            var result = await cmd.ExecuteScalarAsync();
            return result;
        }

    }
}
