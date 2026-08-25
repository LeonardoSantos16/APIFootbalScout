using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Infrastructure.Persistence.Documents
{
    public class UserDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("password_hash")]
        public required string PasswordHash { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("tenant_id")]
        public required string TenantId { get; set; }

        [BsonElement("security_stamp")]
        public required string SecurityStamp { get; set; }

        [BsonElement("roles")]
        public required IReadOnlyCollection<string> Roles { get; set; }

        [BsonElement("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }
    }
}
