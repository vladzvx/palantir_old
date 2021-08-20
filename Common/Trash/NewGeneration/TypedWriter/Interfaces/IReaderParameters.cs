using Npgsql;

namespace Common.Services.DataBase.Interfaces
{
    public interface ICommandParametersSetter<T>
    {
        public void SetParameters(NpgsqlCommand npgsqlCommand);
    }
}
