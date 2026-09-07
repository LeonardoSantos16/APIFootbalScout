using System.ComponentModel.DataAnnotations;
using APIFootballScout.Contracts.Acompanhamento;

namespace APIFootballScout.Contracts.Analise
{
    public sealed record ConsultarMetricasPor90RequestDto
    {
        [Range(1, int.MaxValue)]
        public int CompeticaoId { get; init; }

        [Range(1, int.MaxValue)]
        public int TemporadaId { get; init; }

        [Required]
        [EnumDataType(typeof(ContextoDeRecorteDto))]
        public ContextoDeRecorteDto Contexto { get; init; }
    }
}
