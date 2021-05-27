using Common;
using DataFair.Services.Interfaces;
using DataFair.Utils;
using Microsoft.Extensions.Hosting;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFair.Services.DataBase.DataProcessing
{
    public class DoubledValuesKiller
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ICommonWriter writer;
        private readonly LoadManager loadManager;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString);
        private NpgsqlCommand GetIdCommand;
        private NpgsqlCommand ReadCommand;
        private NpgsqlCommand UpdateChat;
        public DoubledValuesKiller(ICommonWriter writer, LoadManager loadManager)
        {
            this.writer = writer;
            this.loadManager = loadManager;
        }

        private void DBInit()
        {
            connection.Open();
            ReadCommand = connection.CreateCommand();
            ReadCommand.CommandType = System.Data.CommandType.Text;
            ReadCommand.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            ReadCommand.CommandText = "select message_db_id,id from messages where chat_id =@_id;";

            GetIdCommand = connection.CreateCommand();
            GetIdCommand.CommandType = System.Data.CommandType.Text;
            GetIdCommand.CommandText = "select id from chats where last_message_id is not null and last_message_id!=1 and has_doubled;";

            UpdateChat = connection.CreateCommand();
            UpdateChat.CommandType = System.Data.CommandType.Text;
            UpdateChat.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            UpdateChat.CommandText = "update chats set has_doubled=false where id =@_id;";
        }

        public async Task Kill(CancellationToken ct)
        {
            DBInit();
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    long chat_id = 0;
                    using (NpgsqlDataReader reader = GetIdCommand.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            chat_id = reader.GetInt64(0);
                            count++;
                            break;
                        }
                        if (count == 0) return;
                    }

                    if (chat_id == 0) return;
                    HashSet<long> Ids = new HashSet<long>();
                    List<long> ForDelete = new List<long>();
                    ReadCommand.Parameters["_id"].Value = chat_id;
                    using (NpgsqlDataReader reader = ReadCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long message_db_id = reader.GetInt64(0);
                            long id = reader.GetInt64(1);
                            if (Ids.Contains(id))
                            {
                                //Console.WriteLine(string.Format("Deleting. ChatId: {0}; MessageId: {1}; message_db_id: {2}",chat_id,id,message_db_id));
                                await loadManager.WaitIfNeed();
                                writer.PutData(new Deleting() { message_db_id= message_db_id });
                            }
                            else
                            {
                                //Console.WriteLine(string.Format("New value! ChatId: {0}; MessageId: {1}; message_db_id: {2}", chat_id, id, message_db_id));
                                Ids.Add(id);
                            }
                        }
                    }
                    UpdateChat.Parameters["_id"].Value = chat_id;
                    int updates = UpdateChat.ExecuteNonQuery();
                    if (updates != 0)
                        logger.Info(string.Format("Doubled values removed from Chat {0}!",chat_id));

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error while reading data for doubled values killing");
                }
            }
        }
    }
}
