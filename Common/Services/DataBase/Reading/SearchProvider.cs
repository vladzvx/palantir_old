using Common.Services.DataBase.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase
{
    public class SearchProvider
    {
        public readonly ConnectionsFactory connectionPoolManager;
        public readonly ISearchResultReciever searchResultReciever;
        public SearchProvider(ConnectionsFactory connectionPoolManager, ISearchResultReciever searchResultReciever)
        {
            this.connectionPoolManager = connectionPoolManager;
            this.searchResultReciever = searchResultReciever;
        }

        private async Task Search(SearchType storedProcedure,
            string request,
            DateTime startDt,
            DateTime endDt,
            int limit,
            bool is_channel,
            bool is_group,
            CancellationToken token,
            params long[] chat_ids)
        {
            try
            {
                string storedProcedureName = null; ;
                switch (storedProcedure)
                {
                    case SearchType.SearchNamePeriod:
                        storedProcedureName = "search_name_period";
                        break;
                    case SearchType.SearchInChannel:
                        storedProcedureName = "search_in_channel";
                        break;
                    case SearchType.SearchPeriod:
                        storedProcedureName = "search_period";
                        break;
                    default: return;
                }
                using (ConnectionWrapper connectionWrapper = await connectionPoolManager.GetConnectionAsync(token))
                {
                    NpgsqlCommand searchCommand = connectionWrapper.Connection.CreateCommand();
                    searchCommand.CommandText = storedProcedureName;
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

                    if (chat_ids != null && chat_ids.Length > 0 && storedProcedure == SearchType.SearchInChannel)
                    {
                        searchCommand.Parameters.Add(new NpgsqlParameter("ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint));
                        searchCommand.Parameters["ids"].Value = chat_ids;
                    }

                    using NpgsqlDataReader reader = await searchCommand.ExecuteReaderAsync();
                    //List<SearchResult> results = new List<SearchResult>();

                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                        {
                            string name = "";
                            if (reader.FieldCount >= 3)
                            {
                                if (!reader.IsDBNull(2))
                                {
                                    name = reader.GetString(2);
                                }
                            }
                            searchResultReciever.Recieve(new SearchResult()
                            {
                                Link = reader.GetString(0),
                                Text = reader.GetString(1),
                                Name = name
                            }) ;
                        }
                    }
                    //searchResultReciever.IsComplited = true;
                    //return results;
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task CommonSearch(SearchType storedProcedure,
            string request,
            DateTime startDt,
            DateTime endDt,
            int limit,
            bool is_channel,
            bool is_group,
            CancellationToken token,
            params long[] chat_ids)
        {
            await Search(storedProcedure, request, startDt, endDt, 15, is_channel, is_group, token, chat_ids);
            await Search(storedProcedure, request, startDt, endDt, limit, is_channel, is_group, token, chat_ids);


            //double days = Math.Round(endDt.Subtract(startDt).TotalDays);

            //if (days > 2)
            //{
            //    List<Task> tasks = new List<Task>();
            //    int i = 0;
            //    for (i = 1; i <= days && i<=14; i++)
            //    {
            //        tasks.Add(Search(storedProcedure, request, endDt.AddDays(-i), endDt.AddDays(-i+1), limit, is_channel, is_group, token, chat_ids));
            //    }
            //    DateTime dt = endDt.AddDays(-i);
            //    tasks.Add(Search(storedProcedure, request,startDt , dt, limit, is_channel, is_group, token, chat_ids));
            //    Task.WaitAll(tasks.ToArray());
            //}
            //else
            //{
            //    await Search(storedProcedure, request, startDt, endDt, limit, is_channel, is_group, token, chat_ids);
            //}

        }


        public async Task<Task> AsyncSearch(SearchType storedProcedure,
            string request,
            DateTime startDt,
            DateTime endDt,
            int limit,
            bool is_channel,
            bool is_group,
            CancellationToken token,
            params long[] chat_ids)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Search(storedProcedure, request, startDt, endDt, 15, is_channel, is_group, token, chat_ids));
            tasks.Add(Search(storedProcedure, request, startDt, endDt, limit, is_channel, is_group, token, chat_ids));
            await Task.WhenAny(tasks);
            return Task.WhenAll(tasks);

            //double days = Math.Round(endDt.Subtract(startDt).TotalDays);

            //if (days > 2)
            //{
            //    List<Task> tasks = new List<Task>();
            //    int i = 0;
            //    for (i = 1; i <= days && i<=14; i++)
            //    {
            //        tasks.Add(Search(storedProcedure, request, endDt.AddDays(-i), endDt.AddDays(-i+1), limit, is_channel, is_group, token, chat_ids));
            //    }
            //    DateTime dt = endDt.AddDays(-i);
            //    tasks.Add(Search(storedProcedure, request,startDt , dt, limit, is_channel, is_group, token, chat_ids));
            //    Task.WaitAll(tasks.ToArray());
            //}
            //else
            //{
            //    await Search(storedProcedure, request, startDt, endDt, limit, is_channel, is_group, token, chat_ids);
            //}

        }
        public async Task PersonSearch(int limit, long id, CancellationToken token)
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
                    //List<SearchResult> results = new List<SearchResult>();

                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                        {
                            searchResultReciever.Recieve(new SearchResult()
                            {
                                Link = reader.GetString(0),
                                Text = reader.GetString(1)
                            });
                        }
                    }
                    //return results;
                }
            }
            catch (Exception)
            {
                // return new List<SearchResult>();
            }
        }

    }
}
