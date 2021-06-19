using Common.Services.DataBase.Interfaces;
using DataFair.Utils;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.DataProcessing
{
    public class TextVectorizer : IHostedService
    {

        private readonly ConnectionPoolManager connectionPoolManager;
        private readonly CancellationTokenSource globalCts;
        private readonly IDataBaseSettings settings;
        private Task mainTask;
        public TextVectorizer(ConnectionPoolManager connectionPoolManager, CancellationTokenSource globalCts, IDataBaseSettings settings)
        {
            this.connectionPoolManager = connectionPoolManager;
            this.globalCts = globalCts;
            this.settings = settings;
        }

        private static async Task act(NpgsqlCommand mainCommand, NpgsqlCommand additionalCommand, int count,CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    mainCommand.Parameters["count"].Value = count;
                    await mainCommand.ExecuteNonQueryAsync(token);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("memory alloc"))
                    {
                        count /= 10;
                        if (count > 1)
                            await act(mainCommand, additionalCommand, count, token);
                        else
                        {
                            await additionalCommand.ExecuteNonQueryAsync(token);
                        }
                    }
                    return;
                }
            }
        }

        private async Task action(CancellationToken token)
        {
            
            while (!token.IsCancellationRequested)
            {
                using ConnectionWrapper connection = await connectionPoolManager.GetConnection(token);
                try
                {
                    NpgsqlCommand mainCommand = connection.Connection.CreateCommand();
                    mainCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    mainCommand.CommandText = "create_fulltext_data";
                    mainCommand.Parameters.Add(new NpgsqlParameter("count", NpgsqlTypes.NpgsqlDbType.Integer));

                    NpgsqlCommand additionalCommand = connection.Connection.CreateCommand();
                    additionalCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    additionalCommand.CommandText = "force_parse";
                    int count = settings.StartVectorizerCount;
                    await act(mainCommand,additionalCommand,count,token);

                }
                catch { }
                await Task.Delay(100, token);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            action(globalCts.Token).Wait();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
