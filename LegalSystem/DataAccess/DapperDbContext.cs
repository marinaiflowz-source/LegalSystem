using LegalSystem.DTOs;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;

namespace LegalSystem.DataAccess
{
    public class DapperDbContext
    {
        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string connection, string spName, bool isMulti, object? parameters = null)
        {
            var model = new ExecuteQueryModel
            {
                ConnectionString = connection,
                DestinationName = spName,
                CommandType = CommandType.StoredProcedure,
                Parameters = parameters
            };
            var result = isMulti ? await ExecuteMultipleAsync<T>(model) : await ExecuteAsync<T>(model);
            return result;
        }
        public async Task<IEnumerable<T>> ExecuteQueryWithListAsync<T>(string connection, string query, object? parameters = null)
        {
            var model = new ExecuteQueryModel
            {
                ConnectionString = connection,
                DestinationName = query,
                CommandType = CommandType.Text,
                Parameters = parameters
            };
            var result = await ExecuteAsync<T>(model);
            return result;
        }
        public async Task<T?> ExecuteQueryWithFirstAsync<T>(string connection, string query, object? parameters = null)
        {
            var result = await ExecuteQueryWithListAsync<T>(connection, query, parameters);
            return result is null ? default : result.FirstOrDefault();
        }
        public async Task<IEnumerable<T>> ExecuteAsync<T>(ExecuteQueryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ConnectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(model.ConnectionString));

            if (string.IsNullOrWhiteSpace(model.DestinationName))
                throw new ArgumentException("Destination name must be provided.", nameof(model.DestinationName));

            try
            {
                using var connection = new SqlConnection(model.ConnectionString);
                var result = await connection.QueryAsync<T>(
                    model.DestinationName,
                    model.Parameters,
                    commandType: model.CommandType);

                return result;
            }
            catch (SqlException ex)
            {
                // Log exception here
                throw new InvalidOperationException("An error occurred during database operation.", ex);
            }
        }
        public async Task<IEnumerable<T>> ExecuteMultipleAsync<T>(ExecuteQueryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ConnectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(model.ConnectionString));

            if (string.IsNullOrWhiteSpace(model.DestinationName))
                throw new ArgumentException("Destination name must be provided.", nameof(model.DestinationName));

            try
            {
                using var connection = new SqlConnection(model.ConnectionString);
                var multi = await connection.QueryMultipleAsync(
                   model.DestinationName,
                   model.Parameters,
                   commandType: model.CommandType);

                var combinedResults = new List<T>();
                while (!multi.IsConsumed)
                {
                    var resultSet = await multi.ReadAsync<T>();
                    combinedResults.AddRange(resultSet);
                }

                return combinedResults;
            }
            catch (SqlException ex)
            {
                // Log exception here
                throw new InvalidOperationException("An error occurred during database operation.", ex);
            }
        }
        public async Task<long> ExecuteScalarAsync(string connectionString, string query, object model)
        {
            using var connection = new SqlConnection(connectionString);
            var result = await connection.ExecuteScalarAsync<long>(query, model);
            return result;
        }
        public async Task<long> ExecuteCommandAsync(string connectionString, string query, object model)
        {
            using var connection = new SqlConnection(connectionString);
            var result = await connection.ExecuteAsync(query, model);
            return result;
        }
    }
}
