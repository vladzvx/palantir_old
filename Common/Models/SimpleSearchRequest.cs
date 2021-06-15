using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SimpleSearchRequest
    {
        public string Text { get; set; }
        public int Limit { get; set; } = 100;
    }
}
