using Common.Services.DataBase.Interfaces;
using Npgsql;
using System;

namespace Common.Services.DataBase
{
    public class SMReaderCore : IReaderCore1<SavedMessage>
    {
        public bool TryRead(NpgsqlDataReader reader, out SavedMessage data)
        {
            data = new SavedMessage();
            try
            {
                data.MessageDBId = reader.GetInt64(0);
                data.Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(reader.GetDateTime(1));
                data.Id = reader.GetInt64(2);
                data.ChatId = reader.GetInt64(3);
                data.FromId = reader.GetInt64(4);
                data.ReplyTo = reader.IsDBNull(5) ? reader.GetInt64(5) : 0;
                data.ThreadStart = reader.IsDBNull(6) ? reader.GetInt64(6) : 0;
                data.MediagroupId = reader.IsDBNull(7) ? reader.GetInt64(7) : 0;
                data.ForwardFromId = reader.IsDBNull(8) ? reader.GetInt64(8) : 0;
                data.ForwardFromMessageId = reader.IsDBNull(9) ? reader.GetInt64(9) : 0;
                data.Text = reader.IsDBNull(10) ? reader.GetString(10) : string.Empty;
                data.Media = reader.IsDBNull(11) ? reader.GetString(11) : string.Empty;
                data.Formating = reader.IsDBNull(12) ? reader.GetString(12) : string.Empty;
                data.MediaCostyl = reader.IsDBNull(13) ? reader.GetString(13) : string.Empty;
                data.FormatingCostyl = reader.IsDBNull(14) ? reader.GetString(14) : string.Empty;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public NpgsqlCommand CreateCommand(NpgsqlConnection dbConnection)
        {
            NpgsqlCommand DataGetterCommand = dbConnection.CreateCommand();
            DataGetterCommand.CommandType = System.Data.CommandType.Text;
            DataGetterCommand.CommandText = "select * from messages where chat_id=@_id;";
            DataGetterCommand.Parameters.Add("_id");
            return DataGetterCommand;
        }
    }
}
