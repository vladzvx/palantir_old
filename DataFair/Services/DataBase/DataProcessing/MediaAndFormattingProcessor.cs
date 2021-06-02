using DataFair.Utils;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataFair.Services.DataBase.DataProcessing
{
    public class MediaAndFormattingProcessor: IHostedService
    {
        internal class Record
        {
            public long Id;
            public string media_costyl;
            public string formatting_costyl;
        }
        private CancellationTokenSource cts = new CancellationTokenSource();
        private NpgsqlConnection connection = new NpgsqlConnection(Options.ConnectionString);
        private NpgsqlCommand WriteCommand;
        private NpgsqlCommand ReadCommand;
        private readonly DataPreparator dataPreparator;
        public MediaAndFormattingProcessor(DataPreparator dataPreparator)
        {
            this.dataPreparator = dataPreparator;
        }

        private void DBInit()
        {
            connection.Open();
            WriteCommand = connection.CreateCommand();
            WriteCommand.CommandType = System.Data.CommandType.Text;
            WriteCommand.CommandText = "update messages SET formatting_costyl = null, media_costyl = null, formatting = @_formatting, media = @_media where message_db_id = @_id";
            WriteCommand.Parameters.Add(new NpgsqlParameter("_formatting", NpgsqlTypes.NpgsqlDbType.Jsonb));
            WriteCommand.Parameters.Add(new NpgsqlParameter("_media", NpgsqlTypes.NpgsqlDbType.Jsonb));
            WriteCommand.Parameters.Add(new NpgsqlParameter("_id", NpgsqlTypes.NpgsqlDbType.Bigint));


            ReadCommand = connection.CreateCommand();
            ReadCommand.CommandType = System.Data.CommandType.Text;
            ReadCommand.CommandText = "select message_db_id,media_costyl,formatting_costyl from messages where (media_costyl is not null and media_costyl!='') or formatting_costyl is not null limit 10000";
        }

        private void action(object cancellationToken)
        {
            if (!(cancellationToken is CancellationToken)) return;
            CancellationToken ct = (CancellationToken)cancellationToken;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    List<Record> records = new List<Record>();
                    using (NpgsqlDataReader reader = ReadCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Record record = new Record()
                            {
                                Id = reader.GetInt64(0),
                                media_costyl = !reader.IsDBNull(1) ? reader.GetString(1) : null,
                                formatting_costyl = !reader.IsDBNull(2) ? reader.GetString(2) : null,
                            };
                            records.Add(record);
                        }
                    }

                    if (records.Count == 0) return;
                    List<object> list = new List<object>();
                    using NpgsqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        foreach (Record record in records)
                        {
                            object form = dataPreparator.PreparateFormatting(record.formatting_costyl);
                            object med = dataPreparator.PreparateMedia(record.media_costyl);
                            list.Add(form);
                            list.Add(med);
                            WriteCommand.Transaction = transaction;
                            WriteCommand.Parameters["_formatting"].Value = form;
                            WriteCommand.Parameters["_media"].Value = med;
                            WriteCommand.Parameters["_id"].Value = record.Id;
                            WriteCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
    
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DBInit();
            Task.Factory.StartNew(action, cts.Token);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();
            connection.Dispose();
            WriteCommand.Dispose();
            ReadCommand.Dispose();
            return Task.CompletedTask;
        }
    }
}
