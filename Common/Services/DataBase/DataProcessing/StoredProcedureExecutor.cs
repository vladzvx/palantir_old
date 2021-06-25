using Common.Services.DataBase.Interfaces;
using DataFair.Utils;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Common.Services.DataBase.DataProcessing
{

    public class StoredProcedureExecutor : ActionPeriodicExecutor, IHostedService
    {
        private readonly ConnectionsFactory connectionPoolManager;
        private readonly CancellationTokenSource globalCts;
        private readonly IDataBaseSettings settings;
        public StoredProcedureExecutor(ConnectionsFactory connectionPoolManager, CancellationTokenSource globalCts, IDataBaseSettings settings):
            base(settings.StartWritingInterval, globalCts)
        {
            this.connectionPoolManager = connectionPoolManager;
            this.globalCts = globalCts;
            this.settings = settings;
            SetAction(ActionWrapper);
        }

        private void ActionWrapper(object CancellationToken)
        {
            if (CancellationToken is not CancellationToken token) return;
            action(token).Wait();
        }
        private async Task action(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int count = settings.StartVectorizerCount;
                while (!token.IsCancellationRequested)
                {
                    using (ConnectionWrapper connection =await connectionPoolManager.GetConnectionAsync(token))
                    {
                        try
                        {
                            NpgsqlCommand mainCommand = connection.Connection.CreateCommand();
                            mainCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            mainCommand.CommandText = "ban";
                            await mainCommand.ExecuteNonQueryAsync(token);
                        }
                        catch (Exception ex)
                        {

                        }
                        await Task.Delay(100, token);
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
