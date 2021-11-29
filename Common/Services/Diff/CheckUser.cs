using Common.Interfaces;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.Diff
{
    public class UserChecker : IUserChecker
    {
        public async Task<UserInfo> Check(long userId,string text = null)
        {
            UserInfo ft = new UserInfo() { Status = Enums.UserStatus.Normal };
            return ft;
        }

        public async Task<UserInfo> Check(string username)
        {
            return new UserInfo() { Status = Enums.UserStatus.Normal };
        }
    }
}
