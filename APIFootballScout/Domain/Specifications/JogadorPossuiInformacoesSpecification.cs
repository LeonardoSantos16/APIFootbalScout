using APIFootballScout.Domain.Aggregate;
using APIFootballScout.Domain.Base;

namespace APIFootballScout.Domain.Specifications
{
    public sealed class JogadorPossuiInformacoesSpecification : Specification<JogadorDoPerfil>
    {
        public override bool IsSatisfiedBy(JogadorDoPerfil jogador)
            => !string.IsNullOrWhiteSpace(jogador.Nome)
            && jogador.Posicao is not null
            && jogador.Clube is not null;
    }
}
