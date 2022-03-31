using Palantir.FullTextSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palantir.FullTextSearch.Interfaces
{
    public interface ITextPreparator
    {
        public List<PreparatedWord> Preparate(string text, Guid textId, DateTime? timestamp = null)
        {
            List<PreparatedWord> result = new List<PreparatedWord>();
            var words = text.Replace('\n', ' ').Replace(".", "").Replace(",", "").Replace(";", "").Replace("  ", " ").Split(' ');
            ushort position = 0;
            foreach (string word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    result.Add(new PreparatedWord(word, new TextToken.Stored(textId, timestamp, 1, position)));
                    position++;
                }  
            }
            return result;
        }
    }
}
