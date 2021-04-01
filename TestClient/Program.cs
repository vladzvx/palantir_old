using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
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

        private static GrpcChannel Channel;
        private static OrderBoard.OrderBoardClient Client;
        static void Main(string[] args)
        {
            //SendLog("http://176.119.156.220:5020/Send",new LogContainer() {message="ssss" });
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Thread.Sleep(2000);
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            //Channel = GrpcChannel.ForAddress("http://176.119.156.220:5015",new GrpcChannelOptions() {HttpHandler= httpHandler,Credentials=ChannelCredentials.Insecure });
            Channel = GrpcChannel.ForAddress("http://localhost:5005");
            Client = new OrderBoard.OrderBoardClient(Channel);
            var result = Client.GetOrder(new Google.Protobuf.WellKnownTypes.Empty());
        }
    }
}
