using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using Npgsql;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NpgsqlConnection connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("ConnectionString"));
            connection.Open();

            NpgsqlCommand npgsqlCommand = new NpgsqlCommand();
            npgsqlCommand.CommandType = System.Data.CommandType.Text;
            npgsqlCommand.CommandText = "select formatting_costyl, media_costyl from messages where message_db_id = 1075079;";

            using (NpgsqlDataReader reader = npgsqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string text0 = reader.IsDBNull(0) ? null : reader.GetString(0);
                    string text1 = reader.IsDBNull(1) ? null : reader.GetString(1);



                    int q = 0;
                }
            }
        }
    }
}
