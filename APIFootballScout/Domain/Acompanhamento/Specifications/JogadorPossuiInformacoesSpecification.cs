using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Acompanhamento.Aggregate;

namespace APIFootballScout.Domain.Acompanhamento.Specifications
{
    public sealed class JogadorPossuiInformacoesSpecification : Specification<JogadorDoPerfil>
    {
        public override bool IsSatisfiedBy(JogadorDoPerfil jogador)
        {
            return !string.IsNullOrWhiteSpace(jogador.Nome)
                    && jogador.Posicao is not null
                    && jogador.Clube is not null;
        }
    }
}
