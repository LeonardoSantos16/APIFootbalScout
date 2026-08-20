using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Dossie.Aggregate;

namespace APIFootballScout.Domain.Dossie.Specifications
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
