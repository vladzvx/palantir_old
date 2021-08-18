using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Enums
{
    public enum UserStatus
    {
        common = 2,
        master = -1,
        privileged = 0,
        beta_tester = 1,
        banned = 3
    }
}
