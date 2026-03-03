using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Models.Domain
{
    public class ScoutingReporter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int PlayerId { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public List<string> Pros { get; set; }
        public List<string> Cons { get; set; }
        public string Observation { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
