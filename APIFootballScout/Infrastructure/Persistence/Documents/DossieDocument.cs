using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Infrastructure.Persistence.Documents
{
    internal sealed class DossieDocument
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid Id { get; set; }
        [BsonElement("olheiro_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid OlheiroId { get; set; }
        [BsonElement("jogador_id")]
        public int JogadorId { get; set; }
        [BsonElement("status")]
        public int Status { get; set; }
        [BsonElement("aberto_em")]
        public DateTime AbertoEm { get; set; } = DateTime.UtcNow;
        [BsonElement("encerrado_em")]
        public DateTime? EncerradoEm { get; set; }
        [BsonElement("linhaDeBase")]
        public LinhaDeBaseDocument LinhaDeBase { get; set; } = default!;
    }

    internal sealed class LinhaDeBaseDocument
    {
        [BsonElement("medida_em")]
        public DateTime MedidaEm { get; set; }

        [BsonElement("clube")]
        public string Clube { get; set; } = string.Empty;

        [BsonElement("valor_em_centavos")]
        public long ValorEmCentavos { get; set; }

        [BsonElement("moeda")]
        public string Moeda { get; set; } = string.Empty;

        [BsonElement("minutos")]
        public int Minutos { get; set; }

        [BsonElement("competicao_id")]
        public int CompeticaoId { get; set; }

        [BsonElement("temporada_id")]
        public int TemporadaId { get; set; }

        [BsonElement("contexto")]
        public int Contexto { get; set; }
    }
}
