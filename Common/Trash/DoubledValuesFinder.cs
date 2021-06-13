using Common;
using Common.Services.Interfaces;
using DataFair.Utils;
using Microsoft.Extensions.Hosting;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.DataProcessing
{
    public class DoubledValuesFinder
    {

        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString);
        private NpgsqlCommand GetIdCommand;
        private NpgsqlCommand UpdateChat;


        private void DBInit()
        {
            connection.Open();
            GetIdCommand = connection.CreateCommand();
            GetIdCommand.CommandType = System.Data.CommandType.Text;
            GetIdCommand.CommandText = "select id from chats where has_doubled is null and last_message_id !=1;";

            UpdateChat = connection.CreateCommand();
            UpdateChat.CommandType = System.Data.CommandType.Text;
            UpdateChat.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            UpdateChat.CommandText = "update chats set has_doubled=exists(select 1 from messages where chat_id=@_id group by chat_id,id having count(id) >1) where id=@_id and last_message_id is not null and last_message_id !=1 and (has_doubled is null);";
        }

        public async Task Find(CancellationToken ct)
        {
            try
            {
                DBInit();
                List<long> Ids = new List<long>();
                using (NpgsqlDataReader reader = GetIdCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ids.Add( reader.GetInt64(0));
                    }
                }
                int count = 0;
                foreach (long id in Ids)
                {
                    try
                    {
                        UpdateChat.Parameters["_id"].Value = id;
                        await UpdateChat.ExecuteNonQueryAsync(ct);

                    }
                    catch (Exception ex)
                    {
                        count++;
                        if (count > 10) return;
                    }

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while reading data for doubled values killing");
            }
            
        }
    }
}
