using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Palantir.FullTextSearch.Models
{
    public class Chunk
    {
        public readonly Guid ChunkId;
        public readonly DateTime ChunkDate;
        public readonly ImmutableList<TextToken> Tokens;

        public Chunk(Stored storedChunk)
        {
            this.ChunkId = storedChunk.ChunkId;
            this.ChunkDate = storedChunk.ChunkDate;
            this.Tokens = ImmutableList.CreateRange(storedChunk.Tokens.Select(item=>item.ToImmutable()));
        }

        public class Stored
        {
            [BsonId]
            public Guid ChunkId { get; set; }
            public bool IsFull { get; set; }
            public string Word { get; set; }
            public DateTime ChunkDate { get; set; }
            public ConcurrentBag<TextToken.Stored> Tokens { get; set; }

            public Stored(string word, DateTime chunkDate)
            {
                ChunkId = Guid.NewGuid();
                Word = word;
                ChunkDate = chunkDate;
                Tokens = new ConcurrentBag<TextToken.Stored>();
            }

            public void AddToken(TextToken.Stored token)
            {
                Tokens.Add(token);
                if (Tokens.Count > 3000)
                {
                    IsFull = true;
                }
            }
            public Chunk ToImmutable()
            {
                return new Chunk(this);
            }
        }
    }
}
