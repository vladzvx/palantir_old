using Common.Services.DataBase.Interfaces;
using Npgsql;

namespace Common.Services.DataBase
{
    public class SMReaderParameters : ICommandParametersSetter<SavedMessage>
    {
        public long Id;
        public void SetParameters(NpgsqlCommand npgsqlCommand)
        {
            npgsqlCommand.Parameters["_id"].Value = Id;
        }
    }
}
