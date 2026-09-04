using System.ComponentModel.DataAnnotations;

namespace APIFootballScout.Contracts.Acompanhamento
{
    public sealed record ConsultarMudancaAcompanhamentoRequestDto
    {
        [Range(1, int.MaxValue)]
        public int JogadorId { get; init; }

    }
}
