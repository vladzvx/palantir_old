using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair
{
    public class SessionSettings
    {
        public string Host;
        public string Dialect;
        public string Username;
        public string Password;
        public string DatabaseName;
    }

    public class UserInfo
    {
        public string Phone;
        public string SessionName;
        public int FullChannelRequestsCounter;
    }

    public class Collector
    {
        public long ApiId;
        public string ApiHash;
    }
}
