using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataFair.Utils
{
    public class DataPreparator
    {
        internal class Media
        {
            public int type;
            public long id;
            public string content;
        }

        private Regex webRegex = new Regex("{\"__MessageMediaWebPage__\": {\"webpage\": {\"__WebPage__\": {\"id\": (\\d+), \"url\": \"(\\S+)\", ");
        private Regex docRegex = new Regex("{\"__MessageMediaDocument__\": {\"document\": {\"__Document__\": {\"id\": (\\d+), ");
        private Regex pollRegex = new Regex("{\"__MessageMediaPoll__\": {\"poll\": {\"__Poll__\": {\"id\": (\\d+), ");
        private Regex photoRegex = new Regex("{\"__MessageMediaPhoto__\": {\"photo\": {\"__Photo__\": {\"id\": (\\d+)," );
        public object PreparateMedia(string mediaInfo)
        {
            if (string.IsNullOrEmpty(mediaInfo)) return DBNull.Value;
            try
            {
                Media media = new Media();
                Match webMatch = webRegex.Match(mediaInfo);
                Match docMatch = docRegex.Match(mediaInfo);
                Match pollMatch = pollRegex.Match(mediaInfo);
                Match photoMatch = photoRegex.Match(mediaInfo);
                if (webMatch.Success)
                {
                    media.type = 4;
                    media.id = long.Parse(webMatch.Groups[1].Value);
                    media.content = webMatch.Groups[2].Value;
                    //"__WebPage__\": {\"id\": 1258039194170978596, \"url\": \"https://aws.amazon.com/ru/message/5467D2/\", 
                }
                else if (docMatch.Success)
                {
                    media.type = 2;
                    media.id = long.Parse(docMatch.Groups[1].Value);
                }
                else if (pollMatch.Success)
                {
                    media.type = 1;
                    media.id = long.Parse(pollMatch.Groups[1].Value);
                    media.content = mediaInfo;
                }
                else if (photoMatch.Success)
                {
                    media.type = 2;
                    media.id = long.Parse(photoMatch.Groups[1].Value);
                }
                else
                {
                    media.type = 0;
                    media.id = -1;
                    media.content = mediaInfo;
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(media);
            }
            catch { }
            return DBNull.Value;

        }

        public object PreparateFormatting(string formatting)
        {
            if (string.IsNullOrEmpty(formatting)) return DBNull.Value;
            object result = DBNull.Value;
            try
            {
                List<Formating> fmt = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Formating>>(formatting);
                if (Formating.TryGetNonEmpty(fmt, out List<Formating> fmt2))
                {
                    result = "{\"formats\": " + Newtonsoft.Json.JsonConvert.SerializeObject(fmt2) + "}";
                }
            }
            catch { }
            return result;
        }
    }
}
