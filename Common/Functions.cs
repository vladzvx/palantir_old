using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class LogContainer
    {
        public string bot_id { get; set; }
        public string message { get; set; }
    }
    public static class Functions
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
    }
}
