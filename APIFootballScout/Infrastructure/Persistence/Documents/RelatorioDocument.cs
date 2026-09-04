using MongoDB.Bson.Serialization.Attributes;

namespace APIFootballScout.Infrastructure.Persistence.Documents
{
    internal sealed class RelatorioDocument
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid ID { get; set; }
        [BsonElement("olheiro_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid OlheiroId { get; set; }
        [BsonElement("jogador_id")]
        public int JogadorId { get; set; }
        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;
        [BsonElement("nota")]
        public decimal? Nota { get; set; }
        [BsonElement("pontos_positivos")]
        public string[] PontosPositivos { get; set; } = [];
        [BsonElement("pontos_negativos")]
        public string[] PontosNegativos { get; set; } = [];
        [BsonElement("texto")]
        public string Texto { get; set; } = string.Empty;
        [BsonElement("parecer")]
        public string? Parecer { get; set; }
        [BsonElement("observado_em")]
        public DateTime ObservadoEm { get; set; }
        [BsonElement("escrito_em")]
        public DateTime Escrito_em { get; set; }
        [BsonElement("finalizado_em")]
        public DateTime? FinalizadoEm { get; set; }
        [BsonElement("corrige_relatorio_id")]
        public Guid? CorrigeRelatorioId { get; set; }
    }
}
