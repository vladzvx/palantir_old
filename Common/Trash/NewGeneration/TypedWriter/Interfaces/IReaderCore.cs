using Npgsql;

namespace Common.Services.DataBase.Interfaces
{
    public interface IReaderCore1<T> where T : class
    {
        public NpgsqlCommand CreateCommand(NpgsqlConnection dbConnection);
        public bool TryRead(NpgsqlDataReader reader, out T data);
    }
}
