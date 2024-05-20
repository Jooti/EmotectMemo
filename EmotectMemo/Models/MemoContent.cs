using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace EmotectMemo.Models
{
    public class MemoContent
    {
        public string Id { get; set; } = $"{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}-{new Random().NextInt64(0,1000)}";
        public string Body { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
