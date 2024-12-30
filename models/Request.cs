using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RequestsApi.Models
{
    public class Request
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; }
    }
}
