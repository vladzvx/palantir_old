using System;

namespace Common.Services.Interfaces
{
    public interface IGrpcSettings
    {
        public string Url => Environment.GetEnvironmentVariable("GrpcUrl");
    }
}
