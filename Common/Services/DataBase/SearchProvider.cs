using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class SearchProvider
    {
        public SearchProvider()
        {


        }

        public async Task<List<string>> SimpleSearch(string text, int limit)
        {
            try
            {
                NpgsqlConnection Connection = new NpgsqlConnection(Options.ConnectionString);
                await Connection.OpenAsync();
                NpgsqlCommand SimpleSearchCommand = Connection.CreateCommand();
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
                    result.Add(reader.GetString(0));
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<string>() { ex.Message };
            }
        }

    }
}
