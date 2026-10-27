using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Infrastructure.Persistence.Documents
{
    internal sealed class ContadorDeAcompanhamentosDocument
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid OlheiroId { get; set; }
        [BsonElement("ativos")]
        public int Ativos { get; set; }
    }
}