using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace TestClient
{
    class LogContainer
    {
        public string message;
    }
    class Program
    {
         
        public static void SendLog(string url, LogContainer postData)
        {
            try
            {
                string responseFromServer = string.Empty;
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(postData));
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/json";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                using (dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                response.Close();
            }
            catch (Exception) { }

            // return responseFromServer;

        }


        public static object getCPUCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            // will always start at 0
            dynamic firstValue = cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            // now matches task manager reading
            dynamic secondValue = cpuCounter.NextValue();

            return secondValue;

        }

        private static GrpcChannel Channel;
        private static OrderBoard.OrderBoardClient Client;
        static void Main(string[] args)
        {
            var q = getCPUCounter();
            //SendLog("http://176.119.156.220:5020/Send",new LogContainer() {message="ssss" });
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Thread.Sleep(2000);
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
           // Channel = GrpcChannel.ForAddress("http://176.119.156.220:5015",new GrpcChannelOptions() {HttpHandler= httpHandler,Credentials=ChannelCredentials.Insecure });
            Channel = GrpcChannel.ForAddress("http://45.132.17.172:5005", new GrpcChannelOptions() { HttpHandler = httpHandler, Credentials = ChannelCredentials.Insecure });
            Client = new OrderBoard.OrderBoardClient(Channel);
            var result = Client.GetState(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
