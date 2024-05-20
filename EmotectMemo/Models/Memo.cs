using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace EmotectMemo.Models
{
    public class Memo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Key { get; set; } = null!;
        public string SecretKey { get; set; } = null!;

        public MemoContent[] Content { get; set; } = null!;

    }
}
