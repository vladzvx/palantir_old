﻿using Common.Services.DataBase.Interfaces;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.DataProcessing
{

    public class TextVectorizer : ActionPeriodicExecutor, IHostedService
    {
        private readonly ConnectionsFactory connectionPoolManager;
        private readonly CancellationTokenSource globalCts;
        private readonly IDataBaseSettings settings;
        public TextVectorizer(ConnectionsFactory connectionPoolManager, CancellationTokenSource globalCts, IDataBaseSettings settings) :
            base(settings.StartWritingInterval, globalCts)
        {
            this.connectionPoolManager = connectionPoolManager;
            this.globalCts = globalCts;
            this.settings = settings;
            SetAction(ActionWrapper);
        }

        private static async Task act(NpgsqlCommand mainCommand, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await mainCommand.ExecuteNonQueryAsync(token);
            }
        }

        private void ActionWrapper(object CancellationToken)
        {
            if (CancellationToken is not CancellationToken token)
            {
                return;
            }

            action(token).Wait();
        }
        private async Task action(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int count = settings.StartVectorizerCount;
                while (!token.IsCancellationRequested)
                {
                    using (ConnectionWrapper connection = await connectionPoolManager.GetConnectionAsync(token))
                    {
                        try
                        {
                            NpgsqlCommand mainCommand = connection.Connection.CreateCommand();
                            mainCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            mainCommand.CommandText = "create_fulltext_data";
                            mainCommand.Parameters.Add(new NpgsqlParameter("count", NpgsqlTypes.NpgsqlDbType.Integer));
                            mainCommand.Parameters["count"].Value = count;
                            bool continuation = true;
                            while (continuation)
                            {
                                using NpgsqlDataReader reader = await mainCommand.ExecuteReaderAsync(token);
                                while (await reader.ReadAsync(token))
                                {
                                    continuation = reader.GetBoolean(0);
                                }
                            }

                            //await act(mainCommand, token);
                        }
                        catch (Exception)
                        {
                            count /= 10;
                            if (count > 1)
                            {
                                break;
                            }
                            else
                            {
                                NpgsqlCommand additionalCommand = connection.Connection.CreateCommand();
                                additionalCommand.CommandType = System.Data.CommandType.StoredProcedure;
                                additionalCommand.CommandText = "force_parse";
                                await additionalCommand.ExecuteNonQueryAsync(token);
                                count = settings.StartVectorizerCount;
                                break;
                            }
                        }
                        await Task.Delay((int)settings.StartWritingInterval, token);
                    }
                }

            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
