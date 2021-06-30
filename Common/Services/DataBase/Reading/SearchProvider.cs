using Common.Models;
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
        public enum SearchType
        {
            search_period = 0,
            search_name_period = 1,
            search_in_channel = 2,
        }
        public readonly ConnectionsFactory connectionPoolManager;
        public SearchProvider(ConnectionsFactory connectionPoolManager)
        {
            this.connectionPoolManager = connectionPoolManager;
        }

        public async Task<List<SearchResult>> CommonSearch(SearchType storedProcedure,
            string request, 
            DateTime startDt, 
            DateTime endDt,
            int limit, 
            bool is_channel, 
            bool is_group, 
            CancellationToken token, 
            long[] chat_ids =null)
        {
            try
            {

                using (ConnectionWrapper connectionWrapper = await connectionPoolManager.GetConnectionAsync(token))
                {
                    NpgsqlCommand searchCommand = connectionWrapper.Connection.CreateCommand();
                    searchCommand.CommandText = storedProcedure.ToString();
                    searchCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    searchCommand.Parameters.Add(new NpgsqlParameter("request", NpgsqlTypes.NpgsqlDbType.Text));
                    searchCommand.Parameters.Add(new NpgsqlParameter("lim", NpgsqlTypes.NpgsqlDbType.Integer));
                    searchCommand.Parameters.Add(new NpgsqlParameter("_is_group", NpgsqlTypes.NpgsqlDbType.Boolean));
                    searchCommand.Parameters.Add(new NpgsqlParameter("_is_channel", NpgsqlTypes.NpgsqlDbType.Boolean));
                    searchCommand.Parameters.Add(new NpgsqlParameter("dt1", NpgsqlTypes.NpgsqlDbType.Timestamp));
                    searchCommand.Parameters.Add(new NpgsqlParameter("dt2", NpgsqlTypes.NpgsqlDbType.Timestamp));

                    searchCommand.Parameters["request"].Value = request;
                    searchCommand.Parameters["lim"].Value = limit;
                    searchCommand.Parameters["dt1"].Value = startDt;
                    searchCommand.Parameters["dt2"].Value = endDt;
                    searchCommand.Parameters["_is_group"].Value = is_group;
                    searchCommand.Parameters["_is_channel"].Value = is_channel;

                    if (chat_ids != null && storedProcedure == SearchType.search_in_channel)
                    {
                        searchCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlTypes.NpgsqlDbType.Array|NpgsqlTypes.NpgsqlDbType.Bigint));
                        searchCommand.Parameters["ids"].Value = chat_ids;
                    }

                    using NpgsqlDataReader reader = await searchCommand.ExecuteReaderAsync();
                    List<SearchResult> results = new List<SearchResult>();

                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0)&&!reader.IsDBNull(1))
                            results.Add(new SearchResult() {Link= reader.GetString(0),Text = reader.GetString(1)
                          });
                    }
                    return results;
                }
            }
            catch (Exception ex)
            {
                return new List<SearchResult>() { new SearchResult() {Link=ex.StackTrace, Text=ex.Message } };
            }
        }

        public async Task<List<SearchResult>> PersonSearch(int limit, long id,CancellationToken token)
        {
            try
            {
                using (ConnectionWrapper connectionWrapper = await connectionPoolManager.GetConnectionAsync(token))
                {
                    NpgsqlCommand searchCommand = connectionWrapper.Connection.CreateCommand();
                    searchCommand.CommandText = "get_user_messages";
                    searchCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    searchCommand.Parameters.Add(new NpgsqlParameter("_user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
                    searchCommand.Parameters.Add(new NpgsqlParameter("lim", NpgsqlTypes.NpgsqlDbType.Integer));


                    searchCommand.Parameters["_user_id"].Value = id;
                    searchCommand.Parameters["lim"].Value = limit;


                    using NpgsqlDataReader reader = await searchCommand.ExecuteReaderAsync();
                    List<SearchResult> results = new List<SearchResult>();

                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                            results.Add(new SearchResult()
                            {
                                Link = reader.GetString(0),
                                Text = reader.GetString(1)
                            });
                    }
                    return results;
                }
            }
            catch (Exception ex)
            {
                return new List<SearchResult>();
            }
        }

    }
}
