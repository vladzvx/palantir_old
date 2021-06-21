using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class SearchProvider
    {
        public readonly ConnectionsFactory connectionPoolManager;
        public SearchProvider(ConnectionsFactory connectionPoolManager)
        {
            this.connectionPoolManager = connectionPoolManager;
        }

        public async Task<List<string>> SimpleSearch(string text, int limit,CancellationToken token)
        {
            try
            {
                using (ConnectionWrapper connectionWrapper = connectionPoolManager.GetConnection(token))
                {
                    NpgsqlCommand SimpleSearchCommand = connectionWrapper.Connection.CreateCommand();
                    SimpleSearchCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("request", NpgsqlTypes.NpgsqlDbType.Text));
                    SimpleSearchCommand.Parameters.Add(new NpgsqlParameter("lim", NpgsqlTypes.NpgsqlDbType.Integer));
                    SimpleSearchCommand.CommandText = "simple_search";

                    SimpleSearchCommand.Parameters["request"].Value = text;
                    SimpleSearchCommand.Parameters["lim"].Value = limit;
                    using NpgsqlDataReader reader = await SimpleSearchCommand.ExecuteReaderAsync();
                    List<string> result = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                            result.Add(reader.GetString(0));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new List<string>() { ex.Message };
            }
        }

    }
}
