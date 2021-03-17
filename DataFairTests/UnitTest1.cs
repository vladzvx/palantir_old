using DataFair;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;
using System.IO;
using System.Threading;

namespace DataFairTests
{
    internal static class Const
    {
        public static readonly string PostgreSQL = "postgres";
        public static readonly string DBName = "test_bd";
        public static readonly string ConnectionString = "User ID=postgres;Password=qw12cv90;Host=localhost;Port=5432;Database={0};Pooling=true";
    }
    [TestClass]
    public class DBWorkerTests
    {
        private static void ExecuteNonQueryScript(string ConnectionString, string Script)
        {
            using (NpgsqlConnection Connention = new NpgsqlConnection(ConnectionString))
            {
                Connention.Open();
                NpgsqlCommand command = Connention.CreateCommand();
                command.CommandText = Script;
                command.CommandType = System.Data.CommandType.Text;
                command.ExecuteNonQuery();
                Connention.Close();
            }
        }

        internal static void CreateTestDB()
        {
            string script = File.ReadAllText("../../../../DBConfiguring.sql");
            ExecuteNonQueryScript(string.Format(Const.ConnectionString, Const.PostgreSQL), string.Format("create database {0} template template0;", Const.DBName));
            ExecuteNonQueryScript(string.Format(Const.ConnectionString, Const.DBName), script);
        }

        internal static void DropTestDB()
        {
            ExecuteNonQueryScript(string.Format(Const.ConnectionString, Const.PostgreSQL), string.Format("drop database if exists {0};", Const.DBName));
        }


        [TestMethod]
        public void UsersInserting()
        {
            DropTestDB();
            CreateTestDB();
            DBWorker dBWorker = new DataFair.DBWorker(string.Format(Const.ConnectionString, Const.DBName));
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.User });
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.User });
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.User });
            Thread.Sleep(500);
            var wt = dBWorker.CheckEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.User });
            wt.Wait();
            Assert.IsTrue(wt.Result);
            dBWorker.Stop();
            Thread.Sleep(500);
            //DropTestDB();
            
        }

        [TestMethod]
        public void GroupsInserting()
        {
            DropTestDB();
            CreateTestDB();
            DBWorker dBWorker = new DataFair.DBWorker(string.Format(Const.ConnectionString, Const.DBName));
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Group });
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Group });
            Thread.Sleep(500);
            var wt = dBWorker.CheckEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Group });

            wt.Wait();
            Assert.IsTrue(wt.Result);
            dBWorker.Stop();
            Thread.Sleep(500);
            //DropTestDB();

        }


        [TestMethod]
        public void ChannelsInserting()
        {
            DropTestDB();
            CreateTestDB();
            DBWorker dBWorker = new DataFair.DBWorker(string.Format(Const.ConnectionString, Const.DBName));
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Channel });
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Channel });
            Thread.Sleep(500);
            var wt = dBWorker.CheckEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Group });
            wt.Wait();
            Assert.IsTrue(wt.Result);
            dBWorker.Stop();
            Thread.Sleep(500);
        }

        [TestMethod]
        public void  PairEntitiesInserting()
        {
            DropTestDB();
            CreateTestDB();
            DBWorker dBWorker = new DataFair.DBWorker(string.Format(Const.ConnectionString, Const.DBName));
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Channel });
            dBWorker.PutEntity(new Common.Entity() { Id = 12345, Type = Common.EntityType.Group, PairId=1234 });
            Thread.Sleep(500);
            var wt1 = dBWorker.CheckEntity(new Common.Entity() { Id = 12345, Type = Common.EntityType.Group });
            wt1.Wait();
            Assert.IsTrue(wt1.Result);
            var wt2 = dBWorker.CheckEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Channel });
            wt2.Wait();
            Assert.IsTrue(wt2.Result);
            dBWorker.Stop();
            Thread.Sleep(500);
        }

        [TestMethod]
        public void MessagesInserting1()
        {
            DropTestDB();
            CreateTestDB();
            DBWorker dBWorker = new DataFair.DBWorker(string.Format(Const.ConnectionString, Const.DBName));
            dBWorker.PutMessage(new Common.Message() {ChatId=1234,Id=1,Timestamp=Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)});
            Thread.Sleep(500);
            dBWorker.Stop();
            Thread.Sleep(500);
        }

        [TestMethod]
        public void MessagesInserting2()
        {
            DropTestDB();
            CreateTestDB();
            DBWorker dBWorker = new DataFair.DBWorker(string.Format(Const.ConnectionString, Const.DBName));
            dBWorker.PutEntity(new Common.Entity() { Id = 1234, Type = Common.EntityType.Channel });
            Thread.Sleep(500);
            dBWorker.PutMessage(new Common.Message() { ChatId = 1234, Id = 1, Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow) });
            dBWorker.PutMessage(new Common.Message() { ChatId = 1234, Id = 1, Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow) });
            dBWorker.PutMessage(new Common.Message() { ChatId = 1234, Id = 1, Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow) });
            Thread.Sleep(500);
            dBWorker.Stop();
        }

    }
}
