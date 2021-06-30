using Common.Services.DataBase;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SearchRequest
    {
        public SearchProvider.SearchType searchType { get; set; }
        public DateTime startDT { get; set; } = new DateTime(2014, 1, 1);
        public DateTime endDT { get; set; } = new DateTime(2050, 1, 1);
        public string Request { get; set; }
        public int Limit { get; set; } = 100;
        public bool isGroup { get; set; } = true;
        public bool isChannel { get; set; } = false;
        public long[] ChatIds { get; set; } = null;
    }

    public class PersonSearchRequest
    {
        public int Limit { get; set; } = 100;
        public long Id { get; set; }
    }
}
