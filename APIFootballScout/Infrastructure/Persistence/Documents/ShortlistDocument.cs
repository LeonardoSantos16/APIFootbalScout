using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Infrastructure.Persistence.Documents
{
    internal sealed class ShortlistDocument
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("olheiro_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid OlheiroId { get; set; }

        [BsonElement("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonElement("limite_de_alvos")]
        public int LimiteDeAlvos { get; set; }

        [BsonElement("alvos")]
        public AlvoDocument[] Alvos { get; set; } = [];
    }

    internal sealed class AlvoDocument
    {
        [BsonElement("jogador_id")]
        public int JogadorId { get; set; }

        [BsonElement("prioridade")]
        public int Prioridade { get; set; }

        [BsonElement("custo_em_centavos")]
        public long CustoEmCentavos { get; set; }

        [BsonElement("moeda")]
        public string Moeda { get; set; } = string.Empty;
    }
}
