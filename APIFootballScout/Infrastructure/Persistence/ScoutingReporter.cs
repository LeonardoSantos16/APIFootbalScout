using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Domain
{
    public class ScoutingReporter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }
        public int PlayerId { get; set; }
        public required string UserId { get; set; }
        public int Rating { get; set; }
        public required List<string> Pros { get; set; }
        public required List<string> Cons { get; set; }
        public required string Observation { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
