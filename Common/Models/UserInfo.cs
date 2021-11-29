using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class UserInfo
    {
        public double Score { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Normal;
    }
}
