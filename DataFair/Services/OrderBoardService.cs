using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using System.IO;

namespace DataFair.Services
{
    public class OrderBoardService : OrderBoard.OrderBoardBase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Order EmptyOrder = new Order() { Type = OrderType.Empty };
        public async override Task<CheckResult> CheckEntity(Entity entity, ServerCallContext context)
        {
            return new CheckResult() { Result = await Storage.worker.CheckEntity(entity) };
        }

        public override Task<Empty> PostEntity(Entity entity, ServerCallContext context)
        {
            logger.Debug("New entity/ Id: {0}; username: {1}; name: {2}; type: {3};", entity.Id, entity.Link,entity.LastName,entity.Type.ToString());
            Storage.worker.PutEntity(entity);
            return Task.FromResult(new Empty());
        }

        public async override Task<Empty> StreamMessages(IAsyncStreamReader<Message> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Storage.worker.PutMessage(requestStream.Current);
                Message message = requestStream.Current;
               // var cachedEntityInfo = new CachedEntityInfo(message.Id);
                //Storage.Chats.AddOrUpdate(message.ChatId, cachedEntityInfo, (key, old) => cachedEntityInfo);
                logger.Debug("Message. DateTime: {0}; FromId: {1}; Text: {2}; Media: {3};", message.Timestamp,message.FromId, message.Text, message.Media);
            }
            return new Empty();
        }
        public override Task<Order> GetOrder(Empty empty, ServerCallContext context)
        {
            try
            {
                if (Storage.Orders.TryDequeue(out Order order))
                {
                    return Task.FromResult(order);
                }
                else
                {
                    return Task.FromResult(EmptyOrder);
                }
            }
            catch (Exception ex )
            {
                return Task.FromResult(EmptyOrder);
            }

        }

        public override Task<Empty> PostOrder(Order order, ServerCallContext context)
        {
            logger.Debug(string.Format("New order received!  Id: {0}; Field: {1};", order.Id,order.Link));
            Storage.Orders.Enqueue(order);
            return Task.FromResult(new Empty());
        }

        public override Task<StateReport> GetState(Empty request, ServerCallContext context)
        {
            long Memory = 0;
            long Disk = 0;
            foreach (Process proc in Process.GetProcesses())
            {
                Memory += proc.WorkingSet64;
            }
            Memory = Memory / 1024 / 1024;

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    Disk+= d.TotalFreeSpace;
                }
            }
            Disk = Disk / 1024 / 1024 / 1024;
            return Task.FromResult(new StateReport() 
            { 
                Entities = Storage.worker.GetEntitiesNumberInQueue(), 
                Messages = Storage.worker.GetMessagesNumberInQueue(),
                Collectors = Storage.Collectors.Count,
                Users = Storage.Users.Count,
                Sessions = Storage.Sessions.Count,
                Orders = Storage.Orders.Count,
                MemoryUsed = Memory,
                FreeDisk = Disk
            });
        }
    }
}
