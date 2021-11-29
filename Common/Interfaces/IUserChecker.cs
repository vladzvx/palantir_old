using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IUserChecker
    {
        public Task<UserInfo> Check(long userId,string text = null);
        public Task<UserInfo> Check(string username);
    }
}
