using Bot.Core.Interfaces;
using Bot.Core.Models;
using Common.Enums;
using Common.Interfaces;
using Common.Models;
using Common.Services.DataBase;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Core.Services
{
    public class UserChecker : IUserChecker
    {
        private readonly ConnectionsFactory connectionsFactory;
        private readonly IDataStorage<GateKeeperBot> dataStorage;
        public UserChecker(ConnectionsFactory connectionsFactory, IDataStorage<GateKeeperBot> dataStorage)
        {
            this.connectionsFactory = connectionsFactory;
            this.dataStorage = dataStorage;
        }
        public async Task<UserInfo> Check(long userId)
        {
            UserInfo ft = new UserInfo() { Status = UserStatus.Normal };
            using (var cnwr =await connectionsFactory.GetConnectionAsync(CancellationToken.None))
            {
                var comm = cnwr.Connection.CreateCommand();
                comm.CommandText = "check_user";
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.Parameters.Add(new Npgsql.NpgsqlParameter("_user_id",NpgsqlTypes.NpgsqlDbType.Bigint));
                comm.Parameters["_user_id"].Value = userId;
                ObjectId currentId = ObjectId.GenerateNewId();
                ObjectId? nextId = ObjectId.GenerateNewId();
                ObjectId? prevId = null;
                List<Page> pages = new List<Page>() { new Page(currentId, prevId, nextId) { position = Page.Position.First, brouseDonate=false } };
                int count = 0;
                int countBad = 0;
                float summ = 0;
                using (var reader = await comm.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        float rank = reader.GetFloat(0);
                        string text = !reader.IsDBNull(1)?reader.GetString(1):string.Empty;
                        string link = !reader.IsDBNull(2)?reader.GetString(2):string.Empty;
                        string name = !reader.IsDBNull(3)?reader.GetString(3):string.Empty;
                        if (!string.IsNullOrEmpty(text))
                        {
                            count++;
                            if (rank != 100)
                            {
                                countBad++;
                                summ += rank;
                                if (!pages.Last().TryAddResult(name, text, link))
                                {
                                    prevId = currentId;
                                    currentId = nextId.Value;
                                    nextId = ObjectId.GenerateNewId();
                                    pages.Add(new Page(currentId, prevId, nextId) { position = Page.Position.Middle, brouseDonate = false });
                                }
                            }
                        }
                    }
                }
                pages.Last().position = Page.Position.Last;
                await dataStorage.SavePages(pages,CancellationToken.None, TextMessage.defaultClient.BotId.Value);
                ft.FirstPageId = pages.Count > 0 &&!string.IsNullOrEmpty(pages.First().Text) ? pages.First().Id : ObjectId.Empty;
                if (summ != 0)
                {
                    ft.TotalMessages = count;
                    ft.BadMessages = countBad;
                    ft.Score = ((count - countBad) * 100 + summ) / count;
                    ft.Status = UserStatus.Middle;
                }
            }
            return ft;
        }

        public async Task<UserInfo> Check(string username)
        {
            return new UserInfo() { Status = UserStatus.Normal };
        }
    }
}
