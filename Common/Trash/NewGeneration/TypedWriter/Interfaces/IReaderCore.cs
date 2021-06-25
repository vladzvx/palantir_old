using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Services.DataBase.Interfaces
{
    public interface IReaderCore1<T> where T:class
    {
        public NpgsqlCommand CreateCommand(NpgsqlConnection dbConnection);
        public bool TryRead(NpgsqlDataReader reader, out T data);
    }
}
