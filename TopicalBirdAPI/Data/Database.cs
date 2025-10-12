using Npgsql;
using System.Data;

namespace TopicalBirdAPI.Data
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PSQL");
        }

        /// <summary>
        /// Creates and opens a new connection.
        /// </summary>
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        /// <summary>
        /// Executes a query that doesn’t return rows (INSERT, UPDATE, DELETE).
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, conn);

            if (parameters != null)
            {
                foreach (var param in parameters)
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a query and returns a single value (e.g. COUNT(*)).
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, conn);

            if (parameters != null)
            {
                foreach (var param in parameters)
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            return await cmd.ExecuteScalarAsync();
        }

        /// <summary>
        /// Executes a query and returns rows as a DataTable.
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, conn);

            if (parameters != null)
            {
                foreach (var param in parameters)
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }
    }
}
