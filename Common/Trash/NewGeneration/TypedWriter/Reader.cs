using Common;
using Common.Services.DataBase.Interfaces;
using Common.Services.Interfaces;
using DataFair.Utils;
using Microsoft.Extensions.Hosting;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.DataProcessing
{
    public class Reader<T> where T:class
    {
        internal Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ICommonProcessor<T> commonProcessor;
        private readonly IReaderCore1<T> readerCore;
        private readonly ConnectionsFactory connectionPoolManager;
        public Reader(ICommonProcessor<T> commonProcessor, IReaderCore1<T> readerCore, ConnectionsFactory connectionPoolManager)
        {
            this.commonProcessor = commonProcessor;
            this.readerCore = readerCore;
            this.connectionPoolManager = connectionPoolManager;
        }

        public async Task Read(ulong id,CancellationToken ct)
        {
            try
            {
                using ConnectionWrapper connectionWrapper = await connectionPoolManager.GetConnectionAsync(ct);
                using NpgsqlCommand DataGetterCommand = readerCore.CreateCommand(connectionWrapper.Connection);
 
                using (NpgsqlDataReader reader = await DataGetterCommand.ExecuteReaderAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                    {
                        if (readerCore.TryRead(reader, out T data))
                        {
                            commonProcessor.Process(data);
                            if (!commonProcessor.IsResultsOk && commonProcessor.ProcessingsCounter > 400)
                            {
                                break;
                            }
                        }
                    }
                    if (!commonProcessor.IsResultsOk)
                    {

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
