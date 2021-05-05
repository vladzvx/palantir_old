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
         
        }
    }
}
