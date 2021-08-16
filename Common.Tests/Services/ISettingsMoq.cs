using Common.Services.DataBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Services
{
    public class ISettingsMoq : ISettings
    {
        public int OrdersManagerCheckingPeriod => 1 * 1 * 500;
    }
}
