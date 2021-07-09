using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.Interfaces
{
    public interface IGrpcSettings
    {
        public string Url => Environment.GetEnvironmentVariable("GrpcUrl");
    }
}
