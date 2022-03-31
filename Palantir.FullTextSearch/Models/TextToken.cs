using System;
using System.Collections.Generic;
using System.Text;

namespace Palantir.FullTextSearch.Models
{
    public class TextToken
    {
        public readonly Guid TextId;
        public readonly DateTime? TextTimestamp;
        public readonly byte WordWeight;
        public readonly ushort WordPosition;

        public TextToken(Stored stored)
        {
            this.TextId = stored.TextId;
            this.TextTimestamp = stored.TextTimestamp;
            this.WordWeight = stored.WordWeight;
            this.WordPosition = stored.WordPosition;
        }

        private static void CheckData(Guid textId, DateTime? textTimestamp, byte wordWeight, ushort wordPosition)
        {
            if (textId == Guid.Empty || (textTimestamp.HasValue? (textTimestamp == DateTime.MinValue || textTimestamp.Value.Kind != DateTimeKind.Utc):false)) throw new ArgumentException("Bad data for creating TextToken");
        }

        public class Stored
        {
            public Guid TextId { get; set; }
            public DateTime? TextTimestamp { get; set; }
            public byte WordWeight { get; set; }
            public ushort WordPosition { get; set; }

            public Stored(Guid textId, DateTime? textTimestamp, byte wordWeight, ushort wordPosition)
            {
                CheckData(textId, textTimestamp, wordWeight, wordPosition);
                this.TextId = textId;
                this.TextTimestamp = textTimestamp;
                this.WordWeight = wordWeight;
                this.WordPosition = wordPosition;
            }

            public TextToken ToImmutable()
            {
                return new TextToken(this);
            }
        }
    }
}
