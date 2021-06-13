using Common.Services.DataBase.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
