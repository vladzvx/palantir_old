using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using Npgsql;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NpgsqlConnection connection = new NpgsqlConnection("User ID=postgres;Password=QW12cv9001Qw_;Host=45.132.17.172;Port=5432;Database=test_db;Pooling=true;");
            connection.Open();

            NpgsqlCommand npgsqlCommand = connection.CreateCommand();
            npgsqlCommand.CommandType = System.Data.CommandType.Text;
            npgsqlCommand.CommandText = "select formatting_costyl, media_costyl from messages where message_db_id = 1075079;";

            using (NpgsqlDataReader reader = npgsqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string formattings_costyl = reader.IsDBNull(0) ? null : reader.GetString(0);
                    if (formattings_costyl != null)
                    {
                        List<Formating> formattings = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Formating>>(formattings_costyl);
                        bool r = Formating.IsEmpty(formattings);
                    }
                        

                    
                    string media_costyl = reader.IsDBNull(1) ? null : reader.GetString(1);

                    Regex reg = new Regex("(__MessageMediaWebPage__)");
                    if (media_costyl != null&& media_costyl.StartsWith("{\"__MessageMediaWebPage__"))
                    {
                        Regex GetIdReg = new Regex(@"id: \d*");
                        GetIdReg.Match(media_costyl);
                        int qq = 0;
                    }

                    int q = 0;
                }
            }
        }
    }
}
