using Npgsql;
using System;
using System.Threading;

namespace DBWorker
{
    public class DBWorker
    {
        private NpgsqlConnection ReadConnention;
        private NpgsqlConnection UsersWriteConnention;
        private NpgsqlConnection MessagesWriteConnention;

        private Thread MessagesWritingThread;
        private Thread UsersWritingThread;

    }
}
