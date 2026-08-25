using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Infrastructure.Persistence.Documents
{
    public class RefreshTokenDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonElement("token_hash")]
        public required string TokenHash { get; set; }

        [BsonElement("expires_at_utc")]
        public DateTime ExpiresAtUtc { get; set; }

        [BsonElement("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }

        [BsonElement("revoked_at_utc")]
        public DateTime? RevokedAtUtc { get; set; }

        [BsonElement("replaced_by_hash")]
        public string? ReplacedByHash { get; set; }
    }
}
