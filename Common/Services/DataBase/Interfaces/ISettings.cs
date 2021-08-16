using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.DataBase.Interfaces
{
    public interface ISettings
    {
        public int OrdersManagerCheckingPeriod => 1 * 60 * 1000;
    }
}
